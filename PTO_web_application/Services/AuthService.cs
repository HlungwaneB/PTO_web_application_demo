using Supabase;
using Supabase.Gotrue;

namespace PTO_web_application.Services;

public class AuthService
{
    private readonly Supabase.Client _supabase;

    public AuthService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<Session?> LoginAsync(string idNumber, string password)
    {
        try
        {
            // 🟢 THIS IS THE TRICK: Convert ID to fake email
            var email = $"{idNumber.Trim()}@pto.local";
            var session = await _supabase.Auth.SignIn(email, password);
            return session;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreatePasswordAsync(string idNumber, string password)
    {
        try
        {
            var email = $"{idNumber.Trim()}@pto.local";

            // 🟢 Just use the standard SignUp without the extra options object
            var session = await _supabase.Auth.SignUp(email, password);

            return session != null;
        }
        catch (Exception ex)
        {
            // 🟢 Print the exact error to the Visual Studio Output window
            Console.WriteLine($"SUPABASE AUTH ERROR: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _supabase.Auth.SignOut();
    }
}