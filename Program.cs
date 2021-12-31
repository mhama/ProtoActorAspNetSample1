using Microsoft.AspNetCore.StaticFiles;
using Proto;
using Proto.Router;
using System.Linq;

// Actor System およびAggregate Actor初期化
var system = new ActorSystem();
var aggregatorProps = Props.FromProducer(() => new ChatMessageAggregatorActor());

// Aggregatorは複数作成し、各ClientはどれかのAggregatorに送る
int aggregatorsCount = 3;
var aggregators = Enumerable.Range(0, aggregatorsCount).Select((_) => system.Root.Spawn(aggregatorProps)).ToList();

//var aggregatorPid = system.Root.Spawn(aggregatorProps);

/* routerの場合
var routerContext = new RootContext(system);
//var routerProps = routerContext.NewRandomPool(aggregatorProps, 2);
var routerProps = routerContext.NewConsistentHashPool(aggregatorProps, 2, null, 10);
var routerPid = routerContext.Spawn(routerProps);
*/

// AggregatorのPIDを供給する
var wrappedAggregatorPidSelector = new ChatAggregatorActorPIDSelector() {
    //pid = aggregatorPid
    //pid = routerPid
    aggregators = aggregators
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ActorSystem>(system); // Hubインスタンス生成時用のDI設定
builder.Services.AddSingleton<ChatAggregatorActorPIDSelector>(wrappedAggregatorPidSelector); // Hubインスタンス生成時用のDI設定

// CORS許可設定
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
  builder =>
  {
    builder.WithOrigins("https://localhost:7030");
  });
});

/*
builder.Services.AddResponseCompression(opts => {
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
*/

var app = builder.Build();

// CORS許可設定
app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

//app.UseResponseCompression();
app.MapHub<ChatHub>("/chathub");

// Set up custom content types - associating file extension to MIME type

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".unityweb"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.Run();
