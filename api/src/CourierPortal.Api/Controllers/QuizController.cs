using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _service;

    public QuizController(IQuizService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(new { data = await _service.GetAllQuizzesAsync(includeInactive) });

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetQuizByIdAsync(id);
        return result is null ? NotFound(new { error = "Quiz not found" }) : Ok(new { data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
        => Ok(new { data = await _service.CreateQuizAsync(dto) });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizDto dto)
    {
        var result = await _service.UpdateQuizAsync(id, dto);
        return result is null ? NotFound(new { error = "Quiz not found" }) : Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteQuizAsync(id);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Quiz not found" });
    }

    [HttpPost("{quizId}/questions")]
    public async Task<IActionResult> AddQuestion(int quizId, [FromBody] CreateQuizQuestionDto dto)
        => Ok(new { data = await _service.AddQuestionAsync(quizId, dto) });

    [HttpPut("questions/{questionId}")]
    public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuizQuestionDto dto)
    {
        var result = await _service.UpdateQuestionAsync(questionId, dto);
        return result is null ? NotFound(new { error = "Question not found" }) : Ok(new { data = result });
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(int questionId)
    {
        var deleted = await _service.DeleteQuestionAsync(questionId);
        return deleted ? Ok(new { success = true }) : NotFound(new { error = "Question not found" });
    }

    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitQuizAttemptDto dto)
        => Ok(new { data = await _service.SubmitAttemptAsync(dto) });

    [HttpGet("attempts/{attemptId}")]
    public async Task<IActionResult> GetAttempt(int attemptId)
    {
        var result = await _service.GetAttemptAsync(attemptId);
        return result is null ? NotFound(new { error = "Attempt not found" }) : Ok(new { data = result });
    }

    [HttpGet("attempts/courier/{courierId}")]
    public async Task<IActionResult> GetCourierAttempts(int courierId)
        => Ok(new { data = await _service.GetAttemptsByCourierAsync(courierId) });

    [HttpGet("{quizId}/attempts")]
    public async Task<IActionResult> GetQuizAttempts(int quizId)
        => Ok(new { data = await _service.GetAttemptsByQuizAsync(quizId) });
}
