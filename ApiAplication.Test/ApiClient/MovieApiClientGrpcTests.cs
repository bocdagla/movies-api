//using ApiApplication.ApiClients.Abstractions;
//using ApiApplication.ApiClients.Response;
//using ApiApplication.Services.Abstractions;
//using Google.Protobuf.Collections;
//using Google.Protobuf.WellKnownTypes;
//using Grpc.Core;
//using Grpc.Core.Testing;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using ProtoDefinitions;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace ApiApplication.ApiClients.Tests
//{
//    public class MovieApiClientGrpcTests
//    {
//        private readonly Mock<MoviesApi.MoviesApiClient> _apiClientMock;
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<ICacheService> _cacheServiceMock;
//        private readonly IMovieApiClient _movieApiClient;

//        public MovieApiClientGrpcTests()
//        {
//            _apiClientMock = new Mock<MoviesApi.MoviesApiClient>();
//            _configurationMock = new Mock<IConfiguration>();
//            _cacheServiceMock = new Mock<ICacheService>();
//            _movieApiClient = new MovieApiClientGrpc(_apiClientMock.Object, _configurationMock.Object, _cacheServiceMock.Object);
//        }

//        private delegate bool TryParseMock(out showResponse outputValue);
//        [Fact]
//        public async Task GetAsync_WithInvalidMovieId_ReturnsGetMovieResponseFromCache()
//        {
//            // Arrange
//            var cancellationToken = CancellationToken.None;
//            var movieId = "1";
//            var showResponse = new showResponse
//            {
//                Id = movieId,
//                FullTitle = "Movie 1",
//                Rank = "1",
//                Title = "Movie 1",
//                ImDbRating = "8",
//                Year = "2021"
//            };
//            var expectedResponse = new GetMovieResponse
//            {
//                Success = true,
//                Movies = new[]
//                {
//                    new MovieData
//                    {
//                        Id = movieId,
//                        FullTitle = "Movie 1",
//                        Rank = 1,
//                        Title = "Movie 1",
//                        ImdbRating = "8",
//                        Year = new DateTime(2021, 1, 1)
//                    }
//                }
//            };
//            var googleAnyType = new Mock<Any>();
//            showResponse outParse;
            
//            googleAnyType.Setup(a => a.TryUnpack(out outParse))
//                .Callback(new TryParseMock((out showResponse val) => { val = showResponse; return true; }));

//            _apiClientMock
//                .Setup((mock) => mock.GetByIdAsync(It.IsAny<IdRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), cancellationToken))
//                .Returns(new ResponseModel { Data = googleAnyType.Object, Success = false });

//            _cacheServiceMock.Setup(mock => mock.ExistsAsync(movieId))
//                .ReturnsAsync(true);
//            _cacheServiceMock.Setup(mock => mock.GetAsync<showResponse>(movieId))
//                .ReturnsAsync(showResponse);

//            // Act
//            var result = await _movieApiClient.GetAsync(movieId, cancellationToken);

//            // Assert
//            Assert.Equal(expectedResponse.Success, result.Success);
//            Assert.Equal(expectedResponse.Movies.Length, result.Movies.Length);
//            Assert.Equal(expectedResponse.Movies[0].Id, result.Movies[0].Id);
//            Assert.Equal(expectedResponse.Movies[0].FullTitle, result.Movies[0].FullTitle);
//            Assert.Equal(expectedResponse.Movies[0].Rank, result.Movies[0].Rank);
//            Assert.Equal(expectedResponse.Movies[0].Title, result.Movies[0].Title);
//            Assert.Equal(expectedResponse.Movies[0].ImdbRating, result.Movies[0].ImdbRating);
//            Assert.Equal(expectedResponse.Movies[0].Year, result.Movies[0].Year);
//        }

//        [Fact]
//        public async Task GetAsync_WithInvalidMovieIdAndNotInCache_ReturnsGetMovieResponseWithErrors()
//        {
//            // Arrange
//            var cancellationToken = CancellationToken.None;
//            var movieId = "1";
//            var exceptions = new RepeatedField<MoviesApiException>
//                {
//                    new MoviesApiException { StatusCode = 400, Message = "Bad Request" },
//                    new MoviesApiException { StatusCode = 404, Message = "Not Found" }
//                };
//            var responseModel =  new Mock<ResponseModel>();
//            responseModel.Setup(s => s.Exceptions).Returns(exceptions);
//            responseModel.Setup(s => s.Success).Returns(false);
//            var expectedResponse = new GetMovieResponse
//            {
//                Success = false,
//                Errors = new[]
//                {
//                    new MovieError { Code = 400, Message = "Bad Request" },
//                    new MovieError { Code = 404, Message = "Not Found" }
//                }
//            };
//            _apiClientMock.Setup<MoviesApi.MoviesApiClient, AsyncUnaryCall<ResponseModel>>(mock => mock.GetByIdAsync(It.IsAny<IdRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), cancellationToken))
//                .ReturnsAsync(responseModel.Object);
//            _cacheServiceMock.Setup(mock => mock.ExistsAsync(movieId))
//                .ReturnsAsync(false);

//            // Act
//            var result = await _movieApiClient.GetAsync(movieId, cancellationToken);

//            // Assert
//            Assert.Equal(expectedResponse.Success, result.Success);
//            Assert.Equal(expectedResponse.Errors.Length, result.Errors.Length);
//            for (int i = 0; i < expectedResponse.Errors.Length; i++)
//            {
//                Assert.Equal(expectedResponse.Errors[i].Code, result.Errors[i].Code);
//                Assert.Equal(expectedResponse.Errors[i].Message, result.Errors[i].Message);
//            }
//        }
//    }
//}
