# Validation and missing pieces (no database changes)

**Nothing in the database was touched.** No model, table name, column name or `[Table]` / `[Column]` attribute was changed, there is no migration and no SQL to run.
The only data operations used are ones that already existed or use existing columns:
read `applications` / `sites`, insert `notifications`, and update `phone_number` and `email` of the applicant's own row.

## New folder `Validation/` (new files only)
| File | What it does |
|---|---|
| `SaId.cs` | South African ID rules: 13 digits, real date of birth, citizenship digit, check digit, gender and age from the ID. Attribute `[SaIdNumber]`. Switch off the check digit with `"Validation": { "EnforceIdCheckDigit": false }` in `appsettings.Development.json` if you test with made-up numbers. |
| `Rules.cs` | Phone (`+27`, spaces accepted, stored as 0821234567), e-mail, names, initials, password strength (8+ characters, upper, lower, digit). Attributes `[SaCellphone]`, `[PersonName]`, `[Initials]`, `[StrongPassword]`. |
| `FileRules.cs` | Document check: PDF/JPG/PNG, 5 MB, and the real file signature (a renamed .exe is refused). |

## Validation added
* **Reliable information:** the eligibility rules existed but never ran (the class did not implement `IValidatableObject`). Now: income below R3 500 and a number, South African, first-time owner, over 21, first-time subsidy.
* **Personal:** names and initials letters only, gender Male/Female, ID number valid (date, check digit), gender matches the ID, at least 21 years, citizen, one application per ID number, previous step done.
* **Contact:** cellphone format and confirmation, e-mail and confirmation, home cellphone optional but checked, site must exist and still be available, "Do you have dependants? NO" now skips the Dependent screen.
* **Dependent:** every field checked, ID number valid, cannot equal the applicant's ID.
* **Documents:** Certified ID and Proof of income are required (Dependent ID when dependants = YES), real file-type check, an upload that fails no longer continues with "upload-failed".
* **Create password:** required, strong, must not contain the ID number.
* **Submit:** everything is checked again on the server, site still free, no duplicate application, before the Supabase user and the row are created.
* **Login:** ID format, password length, 5 wrong passwords = 15 minute wait (per browser session).
* **Track:** 13-digit ID only.
* **Admin:** Dashboard now needs the admin log-in (it did not), site number / size / GPS checked, duplicate site refused, **a site that applications use cannot be deleted** (protects the tables), CSV export quoted against formula injection, Sites screen now lists occupied sites too.
* **Renew:** only an approved PTO within 2 months of expiry, must still live at the site, notes max 500, expiry date taken from the database.
* **Profile:** mobile and e-mail checked and **saved** (they were not saved before); address and next-of-kin fields are checked but there are no columns for them, so they are not stored.
* `Program.cs`: `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`, so only fields marked `[Required]` are required (before, optional fields such as Home cellphone were silently required).

## Added (missing)
* **PTO Officer assists applicants** with the same Apply screens (no new screens): log in with ID `officer`, you land on the application form, fill it in for the beneficiary, and the officer stays logged in after Submit.
  Create the officer once in Supabase: Authentication > Users > Add user, e-mail `officer@pto.local`, a password, tick "Auto confirm user".
* Small views: error summaries on Dependent, Documents, Renew, Profile and Track; site-delete message on Sites.

## Not done (needs a table change, so left alone)
* Dependants are collected but there is no table to save them in.
* Renewal requests and officer name ("captured by") have no table or column, so a renewal only leaves a notification.
* The Supabase client is registered as one shared singleton, so the signed-in Supabase user is shared by every visitor. Per-user clients or ASP.NET Identity would fix that; it is a bigger change.
* Not compiled: build once in Visual Studio (`Ctrl+Shift+B`) and fix any typo it shows.
