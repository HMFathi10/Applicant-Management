using ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant;
using ApplicantManagement.Application.Features.Applicants.Commands.UpdateApplicant;
using ApplicantManagement.Application.Features.Applicants.Commands.DeleteApplicant;
using ApplicantManagement.Application.Features.Applicants.Queries.GetAllApplicants;
using ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantById;
using ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantsWithFilters;
using ApplicantManagement.Application.Features.Applicants.Queries.SearchApplicants;
using ApplicantManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicantsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApplicantsController> _logger;

        public ApplicantsController(IMediator mediator, ILogger<ApplicantsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedApplicantResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = "")
        {
            _logger.LogInformation("Getting paginated applicants: page {Page}, pageSize {PageSize}, searchTerm {SearchTerm}", page, pageSize, searchTerm);
            try
            {
                var query = new GetAllApplicantsQuery
                {
                    Page = page < 1 ? 1 : page,
                    PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                    SearchTerm = searchTerm
                };
                
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paginated applicants");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
        
        [HttpGet("filter")]
        public async Task<ActionResult<List<Applicant>>> Filter(
            [FromQuery] int? experienceMin = null,
            [FromQuery] int? experienceMax = null,
            [FromQuery] string[] location = null
            )
        {
            _logger.LogInformation("Filtering applicants with parameters: " +
                "experienceMin={ExpMin}, experienceMax={ExpMax}, location={Location}, ratingMin={RatingMin}",
                 experienceMin, experienceMax, location);
            
            try
            {
                var applicantsResponse = await _mediator.Send(new GetAllApplicantsQuery());
                
                // Apply filters
                var filteredApplicants = applicantsResponse.Applicants;
                
                if (experienceMin.HasValue)
                    filteredApplicants = filteredApplicants.Where(a => a.Age >= experienceMin.Value).ToList(); // Using Age as a proxy for experience
                
                if (experienceMax.HasValue)
                    filteredApplicants = filteredApplicants.Where(a => a.Age <= experienceMax.Value).ToList();
                
                
                // Create a new PaginatedApplicantResponse with the filtered applicants
                var response = new PaginatedApplicantResponse
                {
                    Applicants = filteredApplicants,
                    TotalCount = filteredApplicants.Count,
                    PageSize = applicantsResponse.PageSize,
                    CurrentPage = applicantsResponse.CurrentPage,
                    TotalPages = (int)Math.Ceiling(filteredApplicants.Count / (double)applicantsResponse.PageSize)
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering applicants");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Applicant>> GetById(int id)
        {
            _logger.LogInformation("Getting applicant with ID: {ApplicantId}", id);
            try
            {
                var applicant = await _mediator.Send(new GetApplicantByIdQuery(id));
                if (applicant == null)
                {
                    return NotFound();
                }
                return Ok(applicant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting applicant with ID: {ApplicantId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<List<Applicant>>> Search([FromQuery] string query)
        {
            _logger.LogInformation("Searching applicants with query: {Query}", query);
            try
            {
                var searchQuery = new SearchApplicantsQuery(query);
                var applicants = await _mediator.Send(searchQuery);
                
                return Ok(applicants ?? new List<Applicant>());
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for search query: {Query}", query);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching applicants with query: {Query}", query);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("advanced-search")]
        public async Task<ActionResult<List<Applicant>>> AdvancedSearch(
            [FromQuery] string searchTerm = null,
            [FromQuery] int? minAge = null,
            [FromQuery] int? maxAge = null,
            [FromQuery] string countryOfOrigin = null,
            [FromQuery] bool? isHired = null,
            [FromQuery] DateTime? appliedDateFrom = null,
            [FromQuery] DateTime? appliedDateTo = null,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Performing advanced search with parameters: SearchTerm={SearchTerm}, MinAge={MinAge}, MaxAge={MaxAge}, Country={Country}, IsHired={IsHired}, DateFrom={DateFrom}, DateTo={DateTo}, SortBy={SortBy}, Descending={Descending}, Page={Page}, PageSize={PageSize}, IncludeDeleted={IncludeDeleted}",
                searchTerm, minAge, maxAge, countryOfOrigin, isHired, appliedDateFrom, appliedDateTo, sortBy, sortDescending, pageNumber, pageSize, includeDeleted);
            
            try
            {
                var query = new GetApplicantsWithFiltersQuery(
                    searchTerm, minAge, maxAge, countryOfOrigin, isHired, 
                    appliedDateFrom, appliedDateTo, sortBy, sortDescending, 
                    pageNumber, pageSize, includeDeleted);
                
                var applicants = await _mediator.Send(query);
                return Ok(applicants);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for advanced search");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced search");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateApplicantCommand command)
        {
            _logger.LogInformation("Creating new applicant: {ApplicantName} {ApplicantFamilyName}", 
                command.Name, command.FamilyName);
            try
            {
                var id = await _mediator.Send(command);
                _logger.LogInformation("Successfully created applicant with ID: {ApplicantId}", id);
                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating applicant");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateApplicantCommand command)
        {
            if (command == null)
            {
                return BadRequest("Update data cannot be null");
            }

            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating applicant with ID: {ApplicantId}", id);
            
            try
            {
                var result = await _mediator.Send(command);
                
                if (!result)
                {
                    return NotFound($"Applicant with ID {id} not found");
                }

                _logger.LogInformation("Successfully updated applicant with ID: {ApplicantId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for applicant update ID: {ApplicantId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating applicant with ID: {ApplicantId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, [FromQuery] byte[] rowVersion = null, [FromQuery] bool hardDelete = false, [FromQuery] string reason = null)
        {
            if (rowVersion == null || rowVersion.Length == 0)
            {
                return BadRequest("Row version is required for concurrency control");
            }

            _logger.LogInformation("Deleting applicant with ID: {ApplicantId}, HardDelete: {HardDelete}, Reason: {Reason}", 
                id, hardDelete, reason ?? "No reason provided");
            
            try
            {
                var command = new DeleteApplicantCommand(id, rowVersion, hardDelete, reason);
                var result = await _mediator.Send(command);
                
                if (!result)
                {
                    return NotFound($"Applicant with ID {id} not found");
                }

                _logger.LogInformation("Successfully deleted applicant with ID: {ApplicantId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for applicant deletion ID: {ApplicantId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting applicant with ID: {ApplicantId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}