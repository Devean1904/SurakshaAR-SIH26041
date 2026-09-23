using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class Program
{
    static readonly HttpClient http = new() { BaseAddress = new Uri("http://localhost:5000/api/") };
    static string? token;
    static string? currentAdminId;

    static async Task Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   SurakshaAR - Admin CLI");
        Console.WriteLine("========================================\n");

        Console.Write("Backend URL [http://localhost:5000]: ");
        string? url = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(url))
            http.BaseAddress = new Uri(url + "/api/");

        Console.WriteLine("\nCompany-first login: create a company+admin or login with Company ID.\n");
        Console.WriteLine("========================================");
        Console.WriteLine("  1. Create Company + First Admin");
        Console.WriteLine("  2. Login (requires Company ID)");
        Console.WriteLine("  0. Exit");
        Console.WriteLine("========================================");
        Console.Write("Choice: ");
        string? start = Console.ReadLine()?.Trim();

        if (start == "1")
        {
            if (!await CreateCompany()) return;
            if (!await LoginWithCompany()) return;
        }
        else if (start == "2")
        {
            if (!await LoginWithCompany()) return;
        }
        else return;

        while (true)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("  ADMIN MENU");
            Console.WriteLine("========================================");
            Console.WriteLine("  1. List Admins");
            Console.WriteLine("  2. Add Admin");
            Console.WriteLine("  3. Remove Admin");
            Console.WriteLine("  4. Reset All Users");
            Console.WriteLine("  0. Exit");
            Console.WriteLine("========================================");
            Console.Write("Choice: ");
            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1": await ListAdmins(); break;
                case "2": await AddAdmin(); break;
                case "3": await RemoveAdmin(); break;
                case "4": await ResetAllUsers(); break;
                case "0": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    static async Task<bool> LoginWithCompany()
    {
        Console.Write($"Company ID{(createdCompanyId != null ? $" [{createdCompanyId}]" : "")}: ");
        string companyId = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(companyId)) companyId = createdCompanyId ?? "";
        Console.Write("Admin ID: ");
        string userId = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Password: ");
        string password = ReadPassword();

        if (string.IsNullOrEmpty(companyId))
        {
            Console.WriteLine("[FAIL] Company ID is required.");
            return false;
        }
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("[FAIL] Admin ID and Password are required.");
            return false;
        }

        var res = await http.PostAsJsonAsync("auth/login", new { userId, password, companyId });
        var content = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"[FAIL] {content}");
            return false;
        }

        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        token = body.GetProperty("token").GetString();
        currentAdminId = userId;
        if (body.TryGetProperty("companyId", out var cid))
            Console.WriteLine($"[OK] Company: {cid.GetString()}");

        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine($"[OK] Logged in as {userId}");
        return true;
    }

    static async Task<bool> CreateCompany()
    {
        Console.Write("Company ID (leave blank for auto): ");
        string companyId = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Company Name: ");
        string companyName = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Admin ID: ");
        string userId = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Full Name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Password: ");
        string password = ReadPassword();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            Console.WriteLine("[FAIL] Company Name is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("[FAIL] Admin ID, Full Name, and Password are required.");
            return false;
        }

        var res = await http.PostAsJsonAsync("company/create", new
        {
            companyId,
            companyName,
            adminUserId = userId,
            adminName = name,
            adminPassword = password
        });
        var body = await res.Content.ReadAsStringAsync();

        if (res.IsSuccessStatusCode)
        {
            try
            {
                var parsed = JsonDocument.Parse(body);
                if (parsed.RootElement.TryGetProperty("companyId", out var created))
                    createdCompanyId = created.GetString();
            }
            catch { /* keep null */ }

            Console.WriteLine($"[OK] Company created. Use Company ID when logging in.");
            if (!string.IsNullOrEmpty(createdCompanyId))
                Console.WriteLine($"     Company ID: {createdCompanyId}");
            return true;
        }

        Console.WriteLine($"[FAIL] {body}");
        return false;
    }

    static string? createdCompanyId;

    static async Task ListAdmins()
    {
        var res = await http.GetAsync("admin/admins");
        var content = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"\n--- Admins ---");
        if (string.IsNullOrWhiteSpace(content) || !res.IsSuccessStatusCode)
        {
            Console.WriteLine($"  [FAIL] {content}");
            return;
        }
        var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
        Console.WriteLine($"  Count: {list?.Count ?? 0}");
        if (list != null)
            foreach (var a in list)
            {
                var cid = a.TryGetProperty("companyId", out var c) ? c.GetString() : "";
                Console.WriteLine($"  {a.GetProperty("userId").GetString()} | {a.GetProperty("name").GetString()} | company={cid}");
            }
    }

    static async Task AddAdmin()
    {
        Console.Write("Admin ID: ");
        string userId = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Full Name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Password: ");
        string password = ReadPassword();

        var res = await http.PostAsJsonAsync("auth/admin/create", new { userId, name, password });
        var body = await res.Content.ReadAsStringAsync();
        Console.WriteLine(res.IsSuccessStatusCode ? $"[OK] Admin '{userId}' created under your company." : $"[FAIL] {body}");
    }

    static async Task RemoveAdmin()
    {
        await ListAdmins();
        Console.Write("Admin ID to remove: ");
        string userId = Console.ReadLine()?.Trim() ?? "";
        var res = await http.PostAsJsonAsync("admin/admin/remove", new { userId });
        var body = await res.Content.ReadAsStringAsync();
        Console.WriteLine(res.IsSuccessStatusCode ? $"[OK] Admin '{userId}' removed." : $"[FAIL] {body}");
    }

    static async Task ResetAllUsers()
    {
        await ListAdmins();
        Console.Write("Are you sure? This deletes ALL users in YOUR company only (admin/manager/worker). Type YES to confirm: ");
        string confirm = Console.ReadLine()?.Trim() ?? "";
        if (confirm != "YES") { Console.WriteLine("Cancelled."); return; }

        var res = await http.PostAsJsonAsync("auth/admin/reset", new { });
        var body = await res.Content.ReadAsStringAsync();
        Console.WriteLine(res.IsSuccessStatusCode ? $"[OK] {body}" : $"[FAIL] {body}");
    }

    static string ReadPassword()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
                Console.Write("\b \b");
                continue;
            }
            if (key.Key != ConsoleKey.Backspace)
            {
                sb.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return sb.ToString();
    }
}
