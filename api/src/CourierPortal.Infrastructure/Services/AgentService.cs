using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Agent CRUD, NP activation flow (toggle IsNetworkPartner → auto-create
/// tucClient ClientTypeId=3 + MasterUser), and agent search.
/// </summary>
public class AgentService : IAgentService
{
    private readonly AgentsDbContext _db;

    public AgentService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentListDto>> GetAllAsync(int tenantId, string? status = null)
    {
        var query = _db.Agents
            .Where(a => a.TenantId == tenantId && a.RecordStatusId == 1);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        return await query
            .OrderBy(a => a.Name)
            .Select(a => new AgentListDto(
                a.AgentId, a.Name, a.City, a.State, a.Phone, a.Email,
                a.IsNetworkPartner, a.NpPortalEnabled, a.NpTier, a.Ranking, a.Status,
                a.Couriers.Count(c => c.RecordStatusId == 1)
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AgentDetailDto?> GetByIdAsync(int agentId, int tenantId)
    {
        var a = await _db.Agents
            .FirstOrDefaultAsync(x => x.AgentId == agentId && x.TenantId == tenantId && x.RecordStatusId == 1);

        return a is null ? null : MapToDetail(a);
    }

    /// <inheritdoc />
    public async Task<AgentDetailDto> CreateAsync(CreateAgentDto dto, int tenantId)
    {
        var agent = new Agent
        {
            TenantId = tenantId,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Postcode = dto.Postcode,
            Phone = dto.Phone,
            AltPhone = dto.AltPhone,
            Email = dto.Email,
            AltEmail = dto.AltEmail,
            Ranking = dto.Ranking,
            Notes = dto.Notes,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
        return MapToDetail(agent);
    }

    /// <inheritdoc />
    public async Task<AgentDetailDto?> UpdateAsync(int agentId, UpdateAgentDto dto, int tenantId)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId && a.RecordStatusId == 1);

        if (agent is null) return null;

        if (dto.Name is not null) agent.Name = dto.Name;
        if (dto.Address is not null) agent.Address = dto.Address;
        if (dto.City is not null) agent.City = dto.City;
        if (dto.State is not null) agent.State = dto.State;
        if (dto.Postcode is not null) agent.Postcode = dto.Postcode;
        if (dto.Phone is not null) agent.Phone = dto.Phone;
        if (dto.AltPhone is not null) agent.AltPhone = dto.AltPhone;
        if (dto.Email is not null) agent.Email = dto.Email;
        if (dto.AltEmail is not null) agent.AltEmail = dto.AltEmail;
        if (dto.Ranking.HasValue) agent.Ranking = dto.Ranking.Value;
        if (dto.Status is not null) agent.Status = dto.Status;
        if (dto.Notes is not null) agent.Notes = dto.Notes;
        if (dto.Latitude.HasValue) agent.Latitude = dto.Latitude;
        if (dto.Longitude.HasValue) agent.Longitude = dto.Longitude;
        if (dto.DefaultCourierPaymentPercent.HasValue)
            agent.DefaultCourierPaymentPercent = dto.DefaultCourierPaymentPercent;

        agent.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDetail(agent);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int agentId, int tenantId)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId && a.RecordStatusId == 1);

        if (agent is null) return false;

        agent.RecordStatusId = 0; // Soft delete
        agent.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentSearchResultDto>> SearchAsync(
        string query, int tenantId, double? lat = null, double? lng = null)
    {
        var normalised = query.Trim().ToLower();

        var agents = await _db.Agents
            .Where(a => a.TenantId == tenantId && a.RecordStatusId == 1)
            .Where(a => a.Name.ToLower().Contains(normalised)
                        || (a.City != null && a.City.ToLower().Contains(normalised))
                        || (a.Email != null && a.Email.ToLower().Contains(normalised)))
            .Take(50)
            .ToListAsync();

        return agents.Select(a =>
        {
            double? distKm = null;
            double score = 1.0;

            if (lat.HasValue && lng.HasValue && a.Latitude.HasValue && a.Longitude.HasValue)
            {
                distKm = HaversineKm(lat.Value, lng.Value, a.Latitude.Value, a.Longitude.Value);
                score = distKm < 50 ? 2.0 : distKm < 200 ? 1.5 : 1.0;
            }

            if (a.Name.Equals(query, StringComparison.OrdinalIgnoreCase))
                score += 1.0;

            return new AgentSearchResultDto(
                a.AgentId, a.Name, a.City, a.State, a.Email,
                a.IsNetworkPartner, a.Ranking, a.Status, distKm, score
            );
        })
        .OrderByDescending(r => r.Score)
        .ThenBy(r => r.DistanceKm)
        .ToList();
    }

    /// <inheritdoc />
    public async Task<AgentDetailDto?> ActivateNpAsync(int agentId, int tenantId)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId && a.RecordStatusId == 1);

        if (agent is null) return null;

        agent.IsNetworkPartner = true;
        agent.NpPortalEnabled = true;
        agent.NpTier = agent.NpTier == 0 ? 1 : agent.NpTier;
        agent.ModifiedDate = DateTime.UtcNow;

        // TODO: In production, this would call into the existing DFRNT database to:
        // 1. INSERT INTO tucClient (ClientTypeId=3, AgentId, UcclCompanyName, ...) 
        // 2. INSERT INTO MasterUser (Email, Password, IsCourier=0, CurrentTenantId)
        // 3. Set agent.NpClientId to the new tucClient ID
        // For now, we mark the intent and the existing codebase handles the actual DB writes.

        await _db.SaveChangesAsync();
        return MapToDetail(agent);
    }

    /// <inheritdoc />
    public async Task<AgentDetailDto?> DeactivateNpAsync(int agentId, int tenantId)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId && a.RecordStatusId == 1);

        if (agent is null) return null;

        agent.NpPortalEnabled = false;
        agent.ModifiedDate = DateTime.UtcNow;

        // TODO: Deactivate MasterUser records, disable tucClient portal access

        await _db.SaveChangesAsync();
        return MapToDetail(agent);
    }

    private static AgentDetailDto MapToDetail(Agent a) => new(
        a.AgentId, a.TenantId, a.Name, a.Address, a.City, a.State, a.Postcode,
        a.Phone, a.AltPhone, a.Email, a.AltEmail,
        a.IsNetworkPartner, a.NpPortalEnabled, a.NpTier, a.Ranking, a.Status,
        a.Latitude, a.Longitude, a.Notes, a.NpClientId, a.DefaultCourierPaymentPercent,
        a.CreatedDate, a.ModifiedDate
    );

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
