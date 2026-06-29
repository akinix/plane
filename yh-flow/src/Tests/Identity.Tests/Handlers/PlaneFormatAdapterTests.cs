using YH.Framework.Shared.Persistence;

namespace Identity.Tests.Handlers;

public class PlaneFormatAdapterTests
{
    [Fact]
    public void PlanePagedResult_FromPagedResponse_ShouldSetCount()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1", "item2"],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Count.ShouldBe(25);
        result.Results.Count.ShouldBe(2);
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_WithNextPage_ShouldBuildNextUrl()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Next.ShouldNotBeNull();
        result.Next.ShouldContain("cursor=2");
        result.Next.ShouldContain("limit=10");
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_WithPreviousPage_ShouldBuildPreviousUrl()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 3,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Previous.ShouldNotBeNull();
        result.Previous.ShouldContain("cursor=2");
        result.Previous.ShouldContain("limit=10");
        result.Next.ShouldBeNull(); // Last page has no next
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_FirstPage_ShouldHaveNoPrevious()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Previous.ShouldBeNull();
        result.Next.ShouldNotBeNull();
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_LastPage_ShouldHaveNoNext()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 3,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Next.ShouldBeNull();
        result.Previous.ShouldNotBeNull();
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_WithExistingQueryParams_ShouldUseAmpersand()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items?filter=active");

        // Assert
        result.Next.ShouldNotBeNull();
        result.Next.ShouldStartWith("/api/items?filter=active&cursor=");
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_WithAdditionalParams_ShouldAppend()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = ["item1"],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items", "sort=name");

        // Assert
        result.Next.ShouldNotBeNull();
        result.Next.ShouldContain("&sort=name");
    }

    [Fact]
    public void PlanePagedResult_FromPagedResponse_EmptyResults_ShouldStillWork()
    {
        // Arrange
        var paged = new PagedResponse<string>
        {
            Items = [],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0,
            TotalPages = 0,
        };

        // Act
        var result = PlanePagedResultFactory.FromPagedResponse(paged, "/api/items");

        // Assert
        result.Count.ShouldBe(0);
        result.Results.ShouldBeEmpty();
        result.Next.ShouldBeNull();
        result.Previous.ShouldBeNull();
    }
}
