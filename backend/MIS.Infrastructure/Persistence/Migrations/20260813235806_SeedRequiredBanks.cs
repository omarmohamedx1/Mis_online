using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedRequiredBanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "CollectionClientOrganizations"
                    ("Id", "Code", "NameArabic", "NameEnglish", "OrganizationType", "SettingsJson", "IsActive", "CreatedAt")
                VALUES
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562201', 'ADIB', 'مصرف أبو ظبي الإسلامي', 'Abu Dhabi Islamic Bank', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562202', 'AAIB', 'البنك العربي الأفريقي الدولي', 'Arab International Bank', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562203', 'BDC', 'بنك القاهرة', 'Banque du Caire', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562204', 'CAE', 'بنك كريدي أجريكول', 'Credit Agricole Egypt', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562205', 'CIB', 'البنك التجاري الدولي', 'Commercial International Bank (CIB)', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562206', 'BM', 'بنك مصر', 'Banque Misr', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562207', 'ENBD', 'بنك الإمارات دبي الوطني', 'Emirates NBD Egypt', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562208', 'HSBC', 'HSBC مصر', 'HSBC Egypt', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562209', 'ABK', 'البنك الأهلي الكويتي – مصر', 'Al Ahli Bank of Kuwait – Egypt (ABK)', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562210', 'NBK', 'بنك الكويت الوطني – مصر', 'National Bank of Kuwait – Egypt (NBK)', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562211', 'ALEXBANK', 'بنك الإسكندرية', 'AlexBank', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562212', 'QNB', 'بنك QNB الأهلي', 'QNB Egypt', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562213', 'BANK_NXT', 'بنك نكست', 'Bank NXT', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP),
                    ('8d815b92-a9e8-4a7f-9463-4f5c02562214', 'NBE', 'البنك الأهلي المصري', 'National Bank of Egypt (NBE)', 'BANK', '{}', TRUE, CURRENT_TIMESTAMP)
                ON CONFLICT ("Code") DO UPDATE SET
                    "NameArabic" = EXCLUDED."NameArabic",
                    "NameEnglish" = EXCLUDED."NameEnglish",
                    "OrganizationType" = EXCLUDED."OrganizationType",
                    "IsActive" = TRUE,
                    "UpdatedAt" = CURRENT_TIMESTAMP;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bank directory records may already be referenced by operational data.
            // Reverting this migration intentionally preserves them.
        }
    }
}
