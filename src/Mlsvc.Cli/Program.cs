using System.CommandLine;

var rootCommand = new RootCommand("mlsvc - Windows ML Service Manager");

// --- render ---
var renderCommand = new Command("render", "Render WinSW XML from manifest");
var renderServiceId = new Argument<string>("service-id", "Service identifier (e.g. acestep)");
renderCommand.AddArgument(renderServiceId);
renderCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[render] Would generate WinSW XML for '{id}' (not yet implemented)");
}, renderServiceId);

// --- install ---
var installCommand = new Command("install", "Install a service via WinSW");
var installServiceId = new Argument<string>("service-id", "Service identifier");
installCommand.AddArgument(installServiceId);
installCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[install] Would install service '{id}' (not yet implemented)");
}, installServiceId);

// --- uninstall ---
var uninstallCommand = new Command("uninstall", "Uninstall a service via WinSW");
var uninstallServiceId = new Argument<string>("service-id", "Service identifier");
uninstallCommand.AddArgument(uninstallServiceId);
uninstallCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[uninstall] Would uninstall service '{id}' (not yet implemented)");
}, uninstallServiceId);

// --- start ---
var startCommand = new Command("start", "Start a service");
var startServiceId = new Argument<string>("service-id", "Service identifier");
startCommand.AddArgument(startServiceId);
startCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[start] Would start service '{id}' (not yet implemented)");
}, startServiceId);

// --- stop ---
var stopCommand = new Command("stop", "Stop a service");
var stopServiceId = new Argument<string>("service-id", "Service identifier");
stopCommand.AddArgument(stopServiceId);
stopCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[stop] Would stop service '{id}' (not yet implemented)");
}, stopServiceId);

// --- restart ---
var restartCommand = new Command("restart", "Restart a service");
var restartServiceId = new Argument<string>("service-id", "Service identifier");
restartCommand.AddArgument(restartServiceId);
restartCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[restart] Would restart service '{id}' (not yet implemented)");
}, restartServiceId);

// --- status ---
var statusCommand = new Command("status", "Show service status");
var statusServiceId = new Argument<string>("service-id", "Service identifier");
statusCommand.AddArgument(statusServiceId);
statusCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[status] Would show status for '{id}' (not yet implemented)");
}, statusServiceId);

// --- health ---
var healthCommand = new Command("health", "Check service health via HTTP endpoint");
var healthServiceId = new Argument<string>("service-id", "Service identifier");
healthCommand.AddArgument(healthServiceId);
healthCommand.SetHandler((string id) =>
{
    Console.WriteLine($"[health] Would check health for '{id}' (not yet implemented)");
}, healthServiceId);

// Register all commands
rootCommand.AddCommand(renderCommand);
rootCommand.AddCommand(installCommand);
rootCommand.AddCommand(uninstallCommand);
rootCommand.AddCommand(startCommand);
rootCommand.AddCommand(stopCommand);
rootCommand.AddCommand(restartCommand);
rootCommand.AddCommand(statusCommand);
rootCommand.AddCommand(healthCommand);

return await rootCommand.InvokeAsync(args);
