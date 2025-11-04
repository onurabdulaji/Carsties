using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(q => q.Ascending(q => q.Make)),
            "new" => query.Sort(q => q.Descending(q => q.CreatedAt)),
            _ => query.Sort(q => q.Ascending(q => q.AuctionEnd)),
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(q => q.AuctionEnd < DateTime.UtcNow),
            "endingsoon" => query.Match(q => q.AuctionEnd < DateTime.UtcNow.AddHours(6)
            && q.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(q => q.AuctionEnd > DateTime.UtcNow),
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(q => q.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(q => q.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            result = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
