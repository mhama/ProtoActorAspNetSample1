using Microsoft.AspNetCore.StaticFiles;
using Proto;

// Actor System およびAggregate Actor初期化
var system = new ActorSystem();
var props = Props.FromProducer(() => new ChatMessageAggregatorActor());
var aggregatorPid = system.Root.Spawn(props);

// PIDはDI時に区別がつかないのでクラスにラップする
var wrappedAggregatorPid = new ChatAggregatorActorPID() {
    pid = aggregatorPid
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ActorSystem>(system); // Hubインスタンス生成時用のDI設定
builder.Services.AddSingleton<ChatAggregatorActorPID>(wrappedAggregatorPid); // Hubインスタンス生成時用のDI設定

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
