using Amazon.S3;
using Amazon.S3.Model;
using Aspose.Cells;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Service.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Test.UnitTesting.Service.Services;

public class FileServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public FileServiceTests(HttpClient httpClient)
    {
        _httpClient = httpClient;
        //Construct configuration
        var currentDirectory = Directory.GetCurrentDirectory();

        var appsettingsPath =
            Path.Combine(currentDirectory, @"..\..\..\..\FinancialPlanning.WebAPI\appsettings.json");

        var builder = new ConfigurationBuilder().AddJsonFile(appsettingsPath).Build();

        _configuration = builder;
    }

    [Theory]
    [InlineData("Template/Financial Plan_Template.xlsx")]
    [InlineData("Template/Monthly Expense Report_Template.xlsx")]
    public async Task GetFileAsyncTests(string key)
    {
        // Arrange
        var s3Client = new AmazonS3Client(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"],
            Amazon.RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]));
        var fileService = new FileService(s3Client, _configuration);

        var expectedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _configuration["AWS:BucketName"],
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var expectedPreSignedUrl = await s3Client.GetPreSignedURLAsync(expectedRequest);

        //Act
        var result = await fileService.GetFileUrlAsync(key);

        //Assert
        Assert.Equal(expectedPreSignedUrl, result);
    }

    [Fact]
    public async Task UploadPlanAsyncTest()
    {
        // Arrange
        var fileName = "CorrectPlan.xlsx";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Test\UnitTesting\File\" + fileName);
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var mockS3Client = new Mock<IAmazonS3>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockHttpClient = new Mock<HttpClient>();

        var fileService = new FileService(mockS3Client.Object, mockConfiguration.Object);

        var key = @"Test\" + fileName;

        mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        await fileService.UploadFileAsync(key, fileStream);

        // Assert
        mockS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Once);
    }

    [Theory]
    [InlineData("EmptyFile.xlsx", 0, "")]
    [InlineData("WrongExtension.txt", 0, "")]
    [InlineData("NoContent.xlsx", 0, "")]
    [InlineData("MissingColumn.xlsx", 0, "")]
    [InlineData("WrongColumnFormat.xlsx", 0, "")]
    [InlineData("CorrectPlan.xlsx", 0, "")]
    [InlineData("CorrectPlan.xls", 0, "")]
    [InlineData("CorrectPlan.csv", 0, "")]
    [InlineData("CorrectReport.xlsx", 1, "")]
    public void ValidateFileTests(string fileName, byte documentType, string expectedResult)
    {
        // Arrange 
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Test\UnitTesting\File\" + fileName);

        // load file to workbook
        Workbook workbook;
        if (Path.GetExtension(fileName) == ".csv")
        {
            var loadOption = new TxtLoadOptions(LoadFormat.Csv)
            {
                Separator = ';', // data in csv file is separated by semicolon
                ConvertDateTimeData = false // do not convert date time to numeric
            };
            workbook = new Workbook(filePath, loadOption);
        }
        else workbook = new Workbook(filePath);

        // create a dummy file with extension xlsx
        filePath = Path.Combine(Directory.GetCurrentDirectory(),
            @"..\..\..\..\Test\UnitTesting\File\" + Path.GetFileNameWithoutExtension(fileName) + "Dump.xlsx");

        // convert to xlsx
        using var memoryStream = new MemoryStream();
        workbook.Save(memoryStream, SaveFormat.Xlsx);
        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        memoryStream.WriteTo(fileStream);
        fileStream.Close();

        var mockS3Client = new Mock<IAmazonS3>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockHttpClient = new Mock<HttpClient>();
        var fileService = new FileService(mockS3Client.Object, mockConfiguration.Object );

        // Act
        var actualResult = fileService.ValidateFile(memoryStream.ToArray(), documentType);

        // delete dummy file
        File.Delete(filePath);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    public static IEnumerable<object[]> TestData()
    {
        yield return
        [
            "CorrectPlan.xlsx", 0, new List<Expense>
            {
                new()
                {
                    No = 1,
                    Date = new DateTime(2024, 3, 8),
                    Term = "klsjdlk",
                    Department = "Delivery",
                    ExpenseName = "asdasd",
                    CostType = "Operation",
                    UnitPrice = 1,
                    Amount = 1,
                    Currency = "askjdhajsdh",
                    ExchangeRate = 2.0,
                    TotalAmount = 1,
                    ProjectName = "Project Name 2",
                    SupplierName = "Supplier Name 1",
                    PIC = "amndsan"
                },
                new Expense
                {
                    No = 2,
                    Date = new DateTime(2024, 3, 8),
                    Term = "klsjdlk",
                    Department = "Delivery",
                    ExpenseName = "asdasd",
                    CostType = "Operation",
                    UnitPrice = 1,
                    Amount = 1,
                    Currency = "askjdhajsdh",
                    ExchangeRate = 2.0,
                    TotalAmount = 1,
                    ProjectName = "Project Name 2",
                    SupplierName = "Supplier Name 1",
                    PIC = "amndsan",
                    Note = "akjdlkajd"
                }
            }
        ];

        yield return
        [
            "CorrectReport.xlsx", 1, new List<Expense>
            {
                new()
                {
                    No = 1,
                    Date = new DateTime(2023, 3, 8),
                    Term = "asdsa",
                    Department = "Delivery",
                    ExpenseName = "qamdnad",
                    CostType = "Non-Recurring",
                    UnitPrice = 1,
                    Amount = 2,
                    TotalAmount = 2,
                    ProjectName = "Project Name 1",
                    SupplierName = "Supplier Name 4",
                    PIC = "asndjasknd"
                },
                new Expense
                {
                    No = 2,
                    Date = new DateTime(2023, 3, 8),
                    Term = "asdsa",
                    Department = "Delivery",
                    ExpenseName = "qamdnad",
                    CostType = "Non-Recurring",
                    UnitPrice = 1,
                    Amount = 2,
                    TotalAmount = 2,
                    ProjectName = "Project Name 1",
                    SupplierName = "Supplier Name 4",
                    PIC = "asndjasknd",
                    Note = "qopwieo"
                }
            }
        ];
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void ConvertExcelToListTest(string fileName, byte documentType, List<Expense> expectedResult)
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Test\UnitTesting\File\" + fileName);
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var mockS3Client = new Mock<IAmazonS3>();
        var fileService = new FileService(mockS3Client.Object, _configuration);

        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        
        // Act
        var actualResult = fileService.ConvertExcelToList(memoryStream.ToArray(), documentType);

        // Assert
        for (var i = 0; i < expectedResult.Count; i++)
            Assert.Equal(expectedResult[i], actualResult[i]);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task ConvertListToExcelAsyncTest(string _, byte documentType, List<Expense> expenses)
    {
        // Arrange
        var mockS3Client = new Mock<IAmazonS3>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockHttpClient = new Mock<HttpClient>();
        var fileService = new FileService(mockS3Client.Object, mockConfiguration.Object);
        
        // Act
        var stream = await fileService.ConvertListToExcelAsync(expenses, documentType);
        
        // Assert
        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
    } 
}