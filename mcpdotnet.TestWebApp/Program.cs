using mcpdotnet.TestWebApp;
using McpDotNet;
using McpDotNet.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMcpServer()
    .WithHttpListenerSseServerTransport("testserver", 8100);



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// �� API �������е����� public �ҷ������Ͳ���IActionResult �������Ϊ����
await app.UseMcpApiFunctions(typeof(Program).Assembly);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
