using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Marketplace postings CRUD, quote submission, and Auto-Mate agent discovery
/// (query ProspectAgent by location, service type, equipment).
/// </summary>
public class MarketplaceService : IMarketplaceService
{
    private readonly AgentsDbContext _db;

    public MarketplaceService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<PostingDto>> GetPostingsAsync(int tenantId, string? status = null)
    {
        var query = _db.MarketplacePostings
            .Where(p => p.TenantId == tenantId && p.RecordStatusId == 1);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new PostingDto(
                p.PostingId, p.Title, p.Description, p.Region, p.ServiceType,
                p.Volume, p.RequiredVehicles, p.Requirements,
                p.StartDate, p.EndDate, p.Status,
                p.Quotes.Count, p.CreatedDate
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PostingDto?> GetPostingAsync(int postingId, int tenantId)
    {
        return await _db.MarketplacePostings
            .Where(p => p.PostingId == postingId && p.TenantId == tenantId && p.RecordStatusId == 1)
            .Select(p => new PostingDto(
                p.PostingId, p.Title, p.Description, p.Region, p.ServiceType,
                p.Volume, p.RequiredVehicles, p.Requirements,
                p.StartDate, p.EndDate, p.Status,
                p.Quotes.Count, p.CreatedDate
            ))
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<PostingDto> CreatePostingAsync(CreatePostingDto dto, int tenantId)
    {
        var posting = new MarketplacePosting
        {
            TenantId = tenantId,
            Title = dto.Title,
            Description = dto.Description,
            Region = dto.Region,
            ServiceType = dto.ServiceType,
            Volume = dto.Volume,
            RequiredVehicles = dto.RequiredVehicles,
            Requirements = dto.Requirements,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _db.MarketplacePostings.Add(posting);
        await _db.SaveChangesAsync();

        return new PostingDto(
            posting.PostingId, posting.Title, posting.Description, posting.Region,
            posting.ServiceType, posting.Volume, posting.RequiredVehicles, posting.Requirements,
            posting.StartDate, posting.EndDate, posting.Status, 0, posting.CreatedDate
        );
    }

    /// <inheritdoc />
    public async Task<QuoteDto> SubmitQuoteAsync(SubmitQuoteDto dto, int agentId)
    {
        var posting = await _db.MarketplacePostings
            .FirstOrDefaultAsync(p => p.PostingId == dto.PostingId && p.RecordStatusId == 1 && p.Status == "Open");

        if (posting is null)
            throw new InvalidOperationException("Posting not found or not open for quotes.");

        // Check for duplicate quote from same agent
        var exists = await _db.MarketplaceQuotes
            .AnyAsync(q => q.PostingId == dto.PostingId && q.AgentId == agentId && q.Status != "Withdrawn");
        if (exists)
            throw new InvalidOperationException("You have already submitted a quote for this posting.");

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == agentId);

        var quote = new MarketplaceQuote
        {
            PostingId = dto.PostingId,
            AgentId = agentId,
            QuotedRate = dto.QuotedRate,
            RateType = dto.RateType,
            Notes = dto.Notes,
            AvailableFrom = dto.AvailableFrom
        };

        _db.MarketplaceQuotes.Add(quote);
        await _db.SaveChangesAsync();

        return new QuoteDto(
            quote.QuoteId, quote.PostingId, quote.AgentId,
            agent?.Name ?? "Unknown", quote.QuotedRate, quote.RateType,
            quote.Notes, quote.AvailableFrom, quote.Status, quote.CreatedDate
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QuoteDto>> GetQuotesForPostingAsync(int postingId, int tenantId)
    {
        return await _db.MarketplaceQuotes
            .Include(q => q.Agent)
            .Where(q => q.PostingId == postingId && q.Posting.TenantId == tenantId)
            .OrderByDescending(q => q.CreatedDate)
            .Select(q => new QuoteDto(
                q.QuoteId, q.PostingId, q.AgentId, q.Agent.Name,
                q.QuotedRate, q.RateType, q.Notes, q.AvailableFrom,
                q.Status, q.CreatedDate
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DiscoverAgentDto>> DiscoverAgentsAsync(
        string? location, string? serviceType, string? equipment,
        double? lat = null, double? lng = null, int radiusKm = 100)
    {
        var query = _db.ProspectAgents.AsQueryable();

        if (!string.IsNullOrEmpty(location))
        {
            var loc = location.ToLower();
            query = query.Where(p =>
                (p.City != null && p.City.ToLower().Contains(loc))
                || (p.State != null && p.State.ToLower().Contains(loc))
                || (p.CoverageAreas != null && p.CoverageAreas.ToLower().Contains(loc)));
        }

        if (!string.IsNullOrEmpty(serviceType))
        {
            var svc = serviceType.ToLower();
            query = query.Where(p => p.Services != null && p.Services.ToLower().Contains(svc));
        }

        if (!string.IsNullOrEmpty(equipment))
        {
            var eq = equipment.ToLower();
            query = query.Where(p => p.Equipment != null && p.Equipment.ToLower().Contains(eq));
        }

        var results = await query.Take(100).ToListAsync();

        return results.Select(p =>
        {
            double? distKm = null;
            if (lat.HasValue && lng.HasValue && p.Latitude.HasValue && p.Longitude.HasValue)
            {
                distKm = HaversineKm(lat.Value, lng.Value, p.Latitude.Value, p.Longitude.Value);
            }

            return new DiscoverAgentDto(
                p.ProspectAgentId, p.CompanyName, p.ContactName, p.Email, p.Phone,
                p.City, p.State, p.Association, p.Services, p.Equipment,
                p.Certifications, p.CoverageAreas, p.FleetSize, p.IsOnboarded, distKm
            );
        })
        .Where(d => !lat.HasValue || !lng.HasValue || !d.DistanceKm.HasValue || d.DistanceKm <= radiusKm)
        .OrderBy(d => d.DistanceKm ?? double.MaxValue)
        .ToList();
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
