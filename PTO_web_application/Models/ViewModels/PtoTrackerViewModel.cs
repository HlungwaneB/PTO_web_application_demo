namespace PTO_web_application.Models.ViewModels;

/// <summary>
/// Drives the shared tracker (journey bar + timeline). It only READS the status text that is already
/// stored in applications.current_status, so nothing in Supabase changes.
/// </summary>
public class PtoTrackerViewModel
{
    public string Status { get; set; } = "Verifying";
    public DateTime? SubmittedOn { get; set; }
    public DateTime? ValidUntil { get; set; }

    public class Step
    {
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public string State { get; set; } = "next";   // done | now | next | bad
    }

    /// <summary>Segments of the 5-part bar that are green (finished).</summary>
    public int Stage => Status switch
    {
        "Verifying" => 1,
        "Pending" => 2,
        "Approved" => 4,
        "Completed" => 5,
        "Rejected" => 2,
        "Cancelled" => 1,
        _ => 1
    };

    public string Headline => Status switch
    {
        "Verifying" => "Formalisation journey · Your documents are being verified.",
        "Pending" => "Formalisation journey · Waiting for a decision.",
        "Approved" => "Formalisation journey · PTO approved.",
        "Completed" => "Formalisation journey · PTO completed.",
        "Rejected" => "Formalisation journey · Application not approved.",
        "Cancelled" => "Formalisation journey · Application cancelled.",
        _ => "Formalisation journey"
    };

    public List<Step> Steps()
    {
        var s = Status;
        bool rejected = s == "Rejected", cancelled = s == "Cancelled";
        string Date(DateTime? d) => d.HasValue ? d.Value.ToString("dd MMM yyyy") : "";

        var list = new List<Step>
        {
            new() { Title = "Application submitted", Detail = Date(SubmittedOn), State = "done" },
            new() { Title = "Documents verified",
                    Detail = s == "Verifying" ? "In progress" : cancelled ? "" : "All documents verified",
                    State = s == "Verifying" ? "now" : cancelled ? "next" : "done" }
        };

        list.Add(new Step
        {
            Title = rejected ? "Not approved" : cancelled ? "Application cancelled" : "Waiting for a decision",
            Detail = rejected ? "The application was rejected" : cancelled ? "" : s == "Pending" ? "In progress" : "",
            State = rejected ? "bad" : cancelled ? "bad" : s == "Pending" ? "now" : (s == "Approved" || s == "Completed") ? "done" : "next"
        });

        if (!rejected && !cancelled)
        {
            list.Add(new Step
            {
                Title = "PTO approved",
                Detail = s == "Approved" ? (ValidUntil.HasValue ? "Valid until " + Date(ValidUntil) : "") : s == "Completed" ? "Done" : "",
                State = s == "Approved" ? "now" : s == "Completed" ? "done" : "next"
            });
            list.Add(new Step
            {
                Title = "Renewal",
                Detail = s == "Completed" ? "Completed" : ValidUntil.HasValue ? "From " + Date(ValidUntil.Value.AddDays(-30)) : "Not yet due",
                State = s == "Completed" ? "done" : "next"
            });
        }
        return list;
    }
}
