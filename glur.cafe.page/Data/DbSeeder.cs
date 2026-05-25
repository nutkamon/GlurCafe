using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using glur.cafe.page.Models;

namespace glur.cafe.page.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Creates tables that may be missing from older databases (e.g. added after initial deploy).
        /// Safe to run on every startup — uses CREATE TABLE IF NOT EXISTS.
        /// </summary>
        public static void EnsureMissingTables(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Quotations"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Quotations"" PRIMARY KEY AUTOINCREMENT,
                    ""QuotationNumber"" TEXT NOT NULL,
                    ""CustomerName"" TEXT NOT NULL,
                    ""Phone"" TEXT NULL,
                    ""Email"" TEXT NULL,
                    ""Address"" TEXT NULL,
                    ""Notes"" TEXT NULL,
                    ""TotalAmount"" TEXT NOT NULL,
                    ""DiscountPercent"" TEXT NOT NULL,
                    ""NetAmount"" TEXT NOT NULL,
                    ""Status"" TEXT NOT NULL,
                    ""PaymentStatus"" TEXT NOT NULL,
                    ""PaidAmount"" TEXT NOT NULL,
                    ""PaymentMethod"" TEXT NULL,
                    ""PaymentNote"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedAt"" TEXT NOT NULL,
                    ""PaidAt"" TEXT NULL
                );");

            context.Database.ExecuteSqlRaw(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Quotations_QuotationNumber""
                ON ""Quotations"" (""QuotationNumber"");");

            context.Database.ExecuteSqlRaw(@"
                CREATE INDEX IF NOT EXISTS ""IX_Quotations_Status""
                ON ""Quotations"" (""Status"");");

            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""QuotationItems"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_QuotationItems"" PRIMARY KEY AUTOINCREMENT,
                    ""QuotationId"" INTEGER NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""Quantity"" INTEGER NOT NULL,
                    ""Unit"" TEXT NOT NULL,
                    ""UnitPrice"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    CONSTRAINT ""FK_QuotationItems_Quotations_QuotationId""
                        FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""Id"") ON DELETE CASCADE
                );");

            context.Database.ExecuteSqlRaw(@"
                CREATE INDEX IF NOT EXISTS ""IX_QuotationItems_QuotationId""
                ON ""QuotationItems"" (""QuotationId"");");

            // --- BeanTypes ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""BeanTypes"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_BeanTypes"" PRIMARY KEY AUTOINCREMENT,
                    ""Name"" TEXT NOT NULL,
                    ""PricePerKg"" TEXT NOT NULL,
                    ""Notes"" TEXT NULL,
                    ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedAt"" TEXT NOT NULL
                );");
            context.Database.ExecuteSqlRaw(@"
                CREATE INDEX IF NOT EXISTS ""IX_BeanTypes_Name""
                ON ""BeanTypes"" (""Name"");");

            // --- ProductCosts ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""ProductCosts"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ProductCosts"" PRIMARY KEY AUTOINCREMENT,
                    ""ProductName"" TEXT NOT NULL,
                    ""LossRate"" TEXT NOT NULL,
                    ""ProcessingFee"" TEXT NOT NULL,
                    ""CostPerKg"" TEXT NOT NULL,
                    ""SellingPrice"" TEXT NOT NULL DEFAULT '0',
                    ""ProfitPercent"" TEXT NOT NULL DEFAULT '0',
                    ""Notes"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedAt"" TEXT NOT NULL
                );");
            // Add new columns if table already exists (for deployed DBs)
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE ""ProductCosts"" ADD COLUMN ""SellingPrice"" TEXT NOT NULL DEFAULT '0';"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE ""ProductCosts"" ADD COLUMN ""ProfitPercent"" TEXT NOT NULL DEFAULT '0';"); } catch { };

            // --- ProductCostItems ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""ProductCostItems"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ProductCostItems"" PRIMARY KEY AUTOINCREMENT,
                    ""ProductCostId"" INTEGER NOT NULL,
                    ""BeanTypeId"" INTEGER NULL,
                    ""BeanName"" TEXT NOT NULL,
                    ""PricePerKg"" TEXT NOT NULL,
                    ""RatioPercent"" TEXT NOT NULL,
                    CONSTRAINT ""FK_ProductCostItems_ProductCosts_ProductCostId""
                        FOREIGN KEY (""ProductCostId"") REFERENCES ""ProductCosts"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_ProductCostItems_BeanTypes_BeanTypeId""
                        FOREIGN KEY (""BeanTypeId"") REFERENCES ""BeanTypes"" (""Id"") ON DELETE SET NULL
                );");

            // --- Customers ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Customers"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Customers"" PRIMARY KEY AUTOINCREMENT,
                    ""FullName"" TEXT NOT NULL,
                    ""Phone"" TEXT NULL,
                    ""Email"" TEXT NULL,
                    ""Company"" TEXT NULL,
                    ""Address"" TEXT NULL,
                    ""CustomerType"" TEXT NOT NULL DEFAULT 'retail',
                    ""Status"" TEXT NOT NULL DEFAULT 'lead',
                    ""Source"" TEXT NULL,
                    ""Notes"" TEXT NULL,
                    ""Tags"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""UpdatedAt"" TEXT NOT NULL,
                    ""LastContactedAt"" TEXT NULL
                );");
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_Customers_Status"" ON ""Customers"" (""Status"");"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_Customers_CustomerType"" ON ""Customers"" (""CustomerType"");"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_Customers_CreatedAt"" ON ""Customers"" (""CreatedAt"");"); } catch { }

            // --- CustomerInteractions ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""CustomerInteractions"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_CustomerInteractions"" PRIMARY KEY AUTOINCREMENT,
                    ""CustomerId"" INTEGER NOT NULL,
                    ""Type"" TEXT NOT NULL DEFAULT 'note',
                    ""Subject"" TEXT NULL,
                    ""Notes"" TEXT NULL,
                    ""FollowUpDate"" TEXT NULL,
                    ""IsFollowUpDone"" INTEGER NOT NULL DEFAULT 0,
                    ""CreatedByAdmin"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    CONSTRAINT ""FK_CustomerInteractions_Customers_CustomerId""
                        FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE CASCADE
                );");
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_CustomerInteractions_CustomerId"" ON ""CustomerInteractions"" (""CustomerId"");"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_CustomerInteractions_FollowUpDate"" ON ""CustomerInteractions"" (""FollowUpDate"");"); } catch { }

            // --- Transactions (รายรับ-รายจ่าย) ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Transactions"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Transactions"" PRIMARY KEY AUTOINCREMENT,
                    ""Type"" TEXT NOT NULL,
                    ""Category"" TEXT NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    ""Date"" TEXT NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );");

            // --- SaleOrders (ใบเสร็จ) ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""SaleOrders"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_SaleOrders"" PRIMARY KEY AUTOINCREMENT,
                    ""OrderNumber"" TEXT NOT NULL,
                    ""Date"" TEXT NOT NULL,
                    ""ItemsTotal"" TEXT NOT NULL,
                    ""DeliveryFee"" TEXT NOT NULL,
                    ""GrandTotal"" TEXT NOT NULL,
                    ""Note"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL
                );");
            try { context.Database.ExecuteSqlRaw(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SaleOrders_OrderNumber"" ON ""SaleOrders"" (""OrderNumber"");"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_SaleOrders_Date"" ON ""SaleOrders"" (""Date"");"); } catch { }

            // --- SaleOrderItems ---
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""SaleOrderItems"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_SaleOrderItems"" PRIMARY KEY AUTOINCREMENT,
                    ""SaleOrderId"" INTEGER NOT NULL,
                    ""Description"" TEXT NOT NULL,
                    ""Quantity"" TEXT NOT NULL,
                    ""Unit"" TEXT NOT NULL,
                    ""UnitPrice"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    CONSTRAINT ""FK_SaleOrderItems_SaleOrders_SaleOrderId""
                        FOREIGN KEY (""SaleOrderId"") REFERENCES ""SaleOrders"" (""Id"") ON DELETE CASCADE
                );");
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_SaleOrderItems_SaleOrderId"" ON ""SaleOrderItems"" (""SaleOrderId"");"); } catch { }

            // --- New columns added by migrations (safe to re-run) ---
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE ""Quotations"" ADD COLUMN ""CustomerId"" INTEGER NULL;"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_Quotations_CustomerId"" ON ""Quotations"" (""CustomerId"");"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE ""ContactMessages"" ADD COLUMN ""CustomerId"" INTEGER NULL;"); } catch { }
            try { context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ContactMessages_CustomerId"" ON ""ContactMessages"" (""CustomerId"");"); } catch { }
        }

        public static void UpdateContactSettings(ApplicationDbContext context)
        {
            var updates = new Dictionary<string, string>
            {
                ["Phone"]           = "096-224-5194",
                ["Email"]           = "contact@glurcoffee.com",
                ["Address"]         = "602, 36 ถ.พหลโยธิน ตำบลคูคต อำเภอลำลูกกา ปทุมธานี 12130",
                ["WorkingHours"]    = "ทุกวัน: 07:00 - 17:00 น.",
                ["GoogleMapsEmbed"] = "https://maps.google.com/maps?q=13.9666807,100.6335154&z=17&output=embed"
            };

            foreach (var kv in updates)
            {
                var setting = context.SiteSettings.FirstOrDefault(s => s.Key == kv.Key);
                if (setting != null) setting.Value = kv.Value;
            }
            context.SaveChanges();
        }

        public static void UpdatePricingPlans(ApplicationDbContext context)
        {
            var plans = new[]
            {
                new { DisplayOrder = 1, Name = "เมล็ดกาแฟไทย", Price = "150", PriceUnit = "180",
                      Description = "ดอยช้าง · แม่จันใต้ · แม่สลอง · เทพเสด็จ · แม่ฮ่องสอน · ไทยบลูมาเท่น · ดอยวาวี · น่าน · บ่อสี่เหลี่ยม · ปางขอน · แพร่ · เวียดนาม · รัฐฉาน · ลาว",
                      FeaturesJson = "[\"คั่วสดก่อนส่งทุกออร์เดอร์\",\"เลือกระดับการคั่วได้\",\"บรรจุถุง Valve ซีลสูญญากาศ\"]",
                      IsPopular = false, CtaText = "สั่งซื้อเลย" },
                new { DisplayOrder = 2, Name = "เมล็ดกาแฟไทย พิเศษ", Price = "160", PriceUnit = "190",
                      Description = "ขุนลาว · ดอยสะเก็ด · ห้วยตาด · ขุนช่างเคี่ยน · อินทนนท์",
                      FeaturesJson = "[\"ขุนลาว · ดอยสะเก็ด · ห้วยตาด = 160฿\",\"ขุนช่างเคี่ยน · อินทนนท์ = 170฿\",\"คั่วสดก่อนส่งทุกออร์เดอร์\",\"เลือกระดับการคั่วได้\",\"บรรจุถุง Valve ซีลสูญญากาศ\"]",
                      IsPopular = true, CtaText = "สั่งซื้อเลย" },
                new { DisplayOrder = 3, Name = "เมล็ดกาแฟต่างประเทศ", Price = "260", PriceUnit = "325",
                      Description = "Brazil · Colombia · Ethiopia · Honduras · Indonesia · Kenya · Uganda",
                      FeaturesJson = "[\"Single Origin คัดเกรดพิเศษ\",\"คั่วสดก่อนส่งทุกออร์เดอร์\",\"เลือกระดับการคั่วได้\",\"มี Tasting Note ทุกถุง\",\"บรรจุถุง Valve ซีลสูญญากาศ\"]",
                      IsPopular = false, CtaText = "สั่งซื้อเลย" },
            };

            foreach (var p in plans)
            {
                var existing = context.PricingPlans.FirstOrDefault(x => x.DisplayOrder == p.DisplayOrder);
                if (existing != null)
                {
                    existing.Name         = p.Name;
                    existing.Price        = p.Price;
                    existing.PriceUnit    = p.PriceUnit;
                    existing.Description  = p.Description;
                    existing.FeaturesJson = p.FeaturesJson;
                    existing.IsPopular    = p.IsPopular;
                    existing.CtaText      = p.CtaText;
                }
            }
            context.SaveChanges();
        }

        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Services.Any()) return; // Already seeded

            var now = DateTime.Now;

            // ===== Admin User =====
            context.AdminUsers.Add(new AdminUser
            {
                Username = "admin",
                PasswordHash = HashPassword("Admin@1234"),
                DisplayName = "ผู้ดูแลระบบ",
                IsActive = true,
                CreatedAt = now
            });

            // ===== Services (12) — บริการของ GLUR CAFE =====
            context.Services.AddRange(
                new Service { Title = "กาแฟสดทุกเมนู", Description = "กาแฟสดชงโดยบาริสต้ามืออาชีพ ตั้งแต่ Espresso, Latte, Cappuccino ไปจนถึง Signature Drinks", IconClassName = "bi-cup-hot", GradientClass = "grad-1", DisplayOrder = 1 },
                new Service { Title = "โรงคั่วเมล็ดกาแฟ", Description = "คั่วเมล็ดกาแฟสดทุกวัน ควบคุมระดับการคั่วอย่างแม่นยำ ตั้งแต่ Light Roast จนถึง Dark Roast", IconClassName = "bi-fire", GradientClass = "grad-2", DisplayOrder = 2 },
                new Service { Title = "เมล็ดกาแฟ Wholesale", Description = "จำหน่ายเมล็ดกาแฟคั่วสดส่งให้ร้านกาแฟและธุรกิจ ราคาพิเศษสำหรับออร์เดอร์จำนวนมาก", IconClassName = "bi-box-seam", GradientClass = "grad-3", DisplayOrder = 3 },
                new Service { Title = "Barista Training Class", Description = "คอร์สสอนทำกาแฟมืออาชีพ เรียนรู้เทคนิคการชง Latte Art และการเลือกใช้เมล็ดกาแฟ", IconClassName = "bi-mortarboard", GradientClass = "grad-4", DisplayOrder = 4 },
                new Service { Title = "ชา & เครื่องดื่มทางเลือก", Description = "ชาไทย ชาเขียว Matcha Hojicha และเครื่องดื่มสมุนไพร สำหรับคนไม่ดื่มกาแฟ", IconClassName = "bi-flower1", GradientClass = "grad-5", DisplayOrder = 5 },
                new Service { Title = "เค้ก & เบเกอรี่", Description = "เบเกอรี่สดทำทุกวัน เค้กโฮมเมด ครัวซองต์ และขนมปังอบร้อน คู่กาแฟได้ลงตัว", IconClassName = "bi-cake2", GradientClass = "grad-6", DisplayOrder = 6 },
                new Service { Title = "กาแฟ Subscription", Description = "สั่งสมัครสมาชิก รับเมล็ดกาแฟคั่วสดส่งถึงบ้านทุกเดือน เลือกสายพันธุ์และระดับการคั่วได้เอง", IconClassName = "bi-calendar-check", GradientClass = "grad-7", DisplayOrder = 7 },
                new Service { Title = "Private Label Roasting", Description = "บริการคั่วกาแฟและบรรจุภัณฑ์สำหรับแบรนด์ของคุณ พร้อมออกแบบฉลากและสูตรพิเศษ", IconClassName = "bi-tags", GradientClass = "grad-8", DisplayOrder = 8 },
                new Service { Title = "Cold Brew & Batch Coffee", Description = "Cold Brew คั่วเป็นพิเศษ หมักนาน 18 ชั่วโมง และ Batch Coffee สำหรับออฟฟิศหรืองาน Event", IconClassName = "bi-droplet-half", GradientClass = "grad-9", DisplayOrder = 9 },
                new Service { Title = "Coffee Cupping Sessions", Description = "นั่งชิมกาแฟแบบ Professional Cupping เรียนรู้ Flavor Profile ของกาแฟจากแหล่งต่างๆ ทั่วโลก", IconClassName = "bi-search-heart", GradientClass = "grad-10", DisplayOrder = 10 },
                new Service { Title = "Event & Catering Coffee", Description = "บริการกาแฟสดสำหรับงาน Event, สัมมนา, งานแต่งงาน พร้อมบาริสต้าและอุปกรณ์ครบ", IconClassName = "bi-building-check", GradientClass = "grad-1", DisplayOrder = 11 },
                new Service { Title = "จัดส่งทั่วประเทศ", Description = "ส่งเมล็ดกาแฟคั่วสดและผลิตภัณฑ์ทั่วประเทศ บรรจุพิเศษรักษาความสดได้นาน 30 วัน", IconClassName = "bi-truck", GradientClass = "grad-2", DisplayOrder = 12 }
            );

            // ===== Portfolios (12) — เมนูและบรรยากาศ =====
            context.Portfolios.AddRange(
                new Portfolio { ProjectName = "GLUR Signature — Thai Latte", ClientName = "เมนูซิกเนเจอร์", Category = "กาแฟซิกเนเจอร์", IconClassName = "bi-cup-hot-fill", GradientClass = "port-1", DisplayOrder = 1 },
                new Portfolio { ProjectName = "Single Origin — Ethiopia Yirgacheffe", ClientName = "Light Roast", Category = "Single Origin", IconClassName = "bi-globe", GradientClass = "port-2", DisplayOrder = 2 },
                new Portfolio { ProjectName = "Cold Brew Collection", ClientName = "Cold Series", Category = "Cold Brew", IconClassName = "bi-droplet-half", GradientClass = "port-3", DisplayOrder = 3 },
                new Portfolio { ProjectName = "Latte Art — Rosetta & Tulip", ClientName = "บาริสต้าซีรีส์", Category = "Latte Art", IconClassName = "bi-flower2", GradientClass = "port-4", DisplayOrder = 4 },
                new Portfolio { ProjectName = "Matcha Series — Hojicha Latte", ClientName = "ชาพรีเมียม", Category = "ชา & ทางเลือก", IconClassName = "bi-flower1", GradientClass = "port-5", DisplayOrder = 5 },
                new Portfolio { ProjectName = "บรรยากาศร้าน — Morning Vibe", ClientName = "Glur Cafe Space", Category = "บรรยากาศร้าน", IconClassName = "bi-house-heart", GradientClass = "port-6", DisplayOrder = 6 },
                new Portfolio { ProjectName = "Wholesale — Bangkok Roasters Co.", ClientName = "Wholesale Partner", Category = "Wholesale", IconClassName = "bi-box-seam", GradientClass = "port-7", DisplayOrder = 7 },
                new Portfolio { ProjectName = "Private Label — Baan Coffee Brand", ClientName = "Baan Coffee", Category = "Private Label", IconClassName = "bi-tags", GradientClass = "port-8", DisplayOrder = 8 },
                new Portfolio { ProjectName = "Barista Class — Batch 12", ClientName = "GLUR Training", Category = "Barista Class", IconClassName = "bi-mortarboard", GradientClass = "port-9", DisplayOrder = 9 },
                new Portfolio { ProjectName = "Event Coffee — Tech Summit 2025", ClientName = "Tech Summit", Category = "Event Catering", IconClassName = "bi-building-check", GradientClass = "port-10", DisplayOrder = 10 },
                new Portfolio { ProjectName = "เบเกอรี่สด — Croissant & Pain Au Choc", ClientName = "GLUR Bakery", Category = "เบเกอรี่", IconClassName = "bi-cake2", GradientClass = "port-1", DisplayOrder = 11 },
                new Portfolio { ProjectName = "Cupping Session — Sudan Rume", ClientName = "Coffee Lab", Category = "Coffee Cupping", IconClassName = "bi-search-heart", GradientClass = "port-2", DisplayOrder = 12 }
            );

            // ===== Pricing Plans =====
            context.PricingPlans.AddRange(
                new PricingPlan
                {
                    Name = "เมล็ดกาแฟไทย", Price = "150", PriceUnit = "฿/200g  |  180฿/250g",
                    Description = "คัดสรรจากแหล่งปลูกชั้นนำทั่วไทย คั่วสดใหม่ทุกออร์เดอร์",
                    FeaturesJson = "[\"ดอยช้าง · แม่จันใต้ · แม่สลอง\",\"เทพเสด็จ · แม่ฮ่องสอน · ดอยวาวี\",\"น่าน · บ่อสี่เหลี่ยม · ปางขอน\",\"แพร่ · เวียดนาม · รัฐฉาน · ลาว\",\"เลือกระดับการคั่วได้\",\"บรรจุถุง Valve ซีลสูญญากาศ\"]",
                    IsPopular = false, CtaText = "สั่งซื้อเลย", DisplayOrder = 1
                },
                new PricingPlan
                {
                    Name = "เมล็ดกาแฟไทย Premium", Price = "160", PriceUnit = "฿/200g  |  190฿/250g",
                    Description = "แหล่งพิเศษ ขุนลาว · ดอยสะเก็ด · ห้วยตาด · ขุนช่างเคี่ยน · อินทนนท์",
                    FeaturesJson = "[\"ขุนลาว · ดอยสะเก็ด · ห้วยตาด (160฿/200g)\",\"ขุนช่างเคี่ยน · อินทนนท์ (170฿/200g)\",\"ขนาด 250g เริ่มต้น 190฿\",\"คั่วสดก่อนส่งทุกออร์เดอร์\",\"เลือกระดับการคั่วได้\",\"บรรจุถุง Valve ซีลสูญญากาศ\"]",
                    IsPopular = true, CtaText = "สั่งซื้อเลย", DisplayOrder = 2
                },
                new PricingPlan
                {
                    Name = "เมล็ดกาแฟต่างประเทศ", Price = "260", PriceUnit = "฿/200g  |  325฿/250g",
                    Description = "Single Origin นำเข้าจากแหล่งปลูกระดับโลก คัดเกรดพิเศษ",
                    FeaturesJson = "[\"Brazil · Colombia · Ethiopia\",\"Honduras · Indonesia · Kenya · Uganda\",\"คั่วสดก่อนส่งทุกออร์เดอร์\",\"เลือกระดับการคั่วได้\",\"บรรจุถุง Valve ซีลสูญญากาศ\",\"มี Tasting Note ทุกถุง\"]",
                    IsPopular = false, CtaText = "สั่งซื้อเลย", DisplayOrder = 3
                }
            );

            // ===== Process Steps — กระบวนการคั่วกาแฟ =====
            context.ProcessSteps.AddRange(
                new ProcessStep { StepNumber = 1, IconClassName = "bi-globe-asia-australia", Title = "คัดสรรเมล็ด", Description = "คัดเลือกเมล็ดกาแฟจากแหล่งปลูกชั้นนำทั่วโลก ตรวจสอบคุณภาพทุกล็อต" },
                new ProcessStep { StepNumber = 2, IconClassName = "bi-fire", Title = "คั่วด้วยใจ", Description = "ควบคุมอุณหภูมิการคั่วอย่างละเอียด ดึงรสชาติที่ดีที่สุดออกมาจากเมล็ดกาแฟ" },
                new ProcessStep { StepNumber = 3, IconClassName = "bi-cup-hot-fill", Title = "ชงด้วยเทคนิค", Description = "บาริสต้ามืออาชีพชงทุกแก้วด้วยความใส่ใจ สัดส่วนที่สมดุลทุกครั้ง" },
                new ProcessStep { StepNumber = 4, IconClassName = "bi-heart-fill", Title = "ส่งมอบความสุข", Description = "เพลิดเพลินกับกาแฟคุณภาพสูง ทุกแก้วคือประสบการณ์ที่ดีที่สุด" }
            );

            // ===== Why Us Items =====
            context.WhyUsItems.AddRange(
                new WhyUsItem { IconClassName = "bi-globe-asia-australia", Title = "Single Origin คัดสรร", Description = "เมล็ดกาแฟจากแหล่งปลูกชั้นนำ Ethiopia, Colombia, Thailand Doi Chaang และอีกมากมาย", DisplayOrder = 1 },
                new WhyUsItem { IconClassName = "bi-fire", Title = "คั่วสดทุกวัน", Description = "คั่วเมล็ดกาแฟสดทุกวัน ไม่มีการสต็อกนาน รับประกันความสดและกลิ่นหอมสูงสุด", DisplayOrder = 2 },
                new WhyUsItem { IconClassName = "bi-award", Title = "บาริสต้ามืออาชีพ", Description = "ทีมบาริสต้าผ่านการอบรมระดับสากล SCA Certified ความชำนาญกว่า 5 ปี", DisplayOrder = 3 },
                new WhyUsItem { IconClassName = "bi-truck", Title = "Wholesale ทั่วไทย", Description = "ส่งเมล็ดกาแฟให้ร้านค้าและธุรกิจทั่วประเทศ ราคาพิเศษ ส่งตรงเวลา", DisplayOrder = 4 },
                new WhyUsItem { IconClassName = "bi-recycle", Title = "Sustainable Sourcing", Description = "สนับสนุนเกษตรกรท้องถิ่น Trade Fairly และใส่ใจสิ่งแวดล้อมในทุกขั้นตอน", DisplayOrder = 5 },
                new WhyUsItem { IconClassName = "bi-shield-check", Title = "รับประกันความพึงพอใจ", Description = "หากไม่พอใจรสชาติ เราพร้อมเปลี่ยนหรือคืนเงิน ไม่มีเงื่อนไข", DisplayOrder = 6 }
            );

            // ===== Stat Items =====
            context.StatItems.AddRange(
                new StatItem { Value = "5+", Label = "ปีแห่งประสบการณ์", IconClassName = "bi-award", DisplayOrder = 1 },
                new StatItem { Value = "50+", Label = "สายพันธุ์กาแฟ", IconClassName = "bi-globe", DisplayOrder = 2 },
                new StatItem { Value = "500+", Label = "ลูกค้า Wholesale", IconClassName = "bi-shop", DisplayOrder = 3 },
                new StatItem { Value = "10,000+", Label = "ถ้วยต่อเดือน", IconClassName = "bi-cup-hot", DisplayOrder = 4 }
            );

            // ===== Site Settings =====
            context.SiteSettings.AddRange(
                new SiteSetting { Key = "Phone", Value = "096-224-5194", Group = "Contact" },
                new SiteSetting { Key = "Email", Value = "contact@glurcoffee.com", Group = "Contact" },
                new SiteSetting { Key = "LineId", Value = "@glurcafe", Group = "Contact" },
                new SiteSetting { Key = "Address", Value = "602, 36 ถ.พหลโยธิน ตำบลคูคต อำเภอลำลูกกา ปทุมธานี 12130", Group = "Contact" },
                new SiteSetting { Key = "WorkingHours", Value = "ทุกวัน: 07:00 - 17:00 น.", Group = "Contact" },
                new SiteSetting { Key = "Facebook", Value = "#", Group = "Social" },
                new SiteSetting { Key = "Line", Value = "https://line.me/ti/p/~glurcafe", Group = "Social" },
                new SiteSetting { Key = "Instagram", Value = "#", Group = "Social" },
                new SiteSetting { Key = "TikTok", Value = "#", Group = "Social" },
                new SiteSetting { Key = "GoogleMapsEmbed", Value = "https://maps.google.com/maps?q=13.9666807,100.6335154&z=17&output=embed", Group = "General" }
            );

            // ===== Contact Messages =====
            var serviceTypes = new[] { "กาแฟสด", "เมล็ดกาแฟ Wholesale", "Barista Class", "Subscription", "Private Label", "Event Catering", "Cold Brew", "ชา & ทางเลือก" };
            var msgData = new[]
            {
                ("สมชาย กาแฟรัก",        "081-111-2233", "somchai@email.com",   1, "สนใจสั่งเมล็ดกาแฟ Ethiopia คั่วกลาง ปริมาณ 5kg/สัปดาห์ สำหรับร้านกาแฟ"),
                ("วิไลพร ใจดี",          "082-222-3344", "wilaiporn@gmail.com",  3, "อยากสมัคร Subscription รับเมล็ดกาแฟทุกเดือน ขอดูรายละเอียดเพิ่มเติม"),
                ("อนุชา ทองดี",          "083-333-4455", "",                     2, "สอบถามราคาคอร์ส Barista สำหรับทีมงาน 5 คน"),
                ("ภัทรา จันทร์เพ็ญ",    "084-444-5566", "phattra@cafe.com",     4, "ต้องการทำ Private Label กาแฟสำหรับร้านเราเอง ขอใบเสนอราคา"),
                ("ธีรพล วงศ์สุวรรณ",    "085-555-6677", "thiraphon@work.co.th", 5, "จองบริการกาแฟสำหรับงาน Seminar 200 คน วันที่ 15 เมษายน"),
                ("สุดารัตน์ พันธ์ดี",    "086-666-7788", "sudarat@hotel.com",    1, "สนใจ Wholesale เมล็ดกาแฟ Colombia สำหรับโรงแรม"),
                ("กิตติศักดิ์ ใจดี",     "087-777-8899", "",                     6, "สั่ง Cold Brew Concentrate 10 ลิตร สำหรับร้านของหวาน"),
                ("พรรณนิภา สมใจ",        "088-888-9900", "pannipa@mail.com",     3, "สนใจเรียน Barista แบบตัวต่อตัว มีหลักสูตรไหนบ้าง"),
                ("วรพจน์ แสนสุข",        "089-999-0011", "worapot@biz.th",       1, "ต้องการเมล็ดกาแฟดอยช้าง คั่ว Light-Medium ปริมาณ 2kg"),
                ("ชนัญชิดา รักษ์ดี",    "081-234-5678", "chananchida@shop.th",  7, "อยากลองสั่ง Matcha Latte และชาไทยเย็น สำหรับร้านของเรา"),
                ("มนตรี ศิริวัฒน์",      "082-345-6789", "",                     4, "ต้องการทำแบรนด์กาแฟเอง ขอคุยเรื่อง Private Label Roasting"),
                ("จิตรา บุญมาก",         "083-456-7890", "jittra@office.co.th",  5, "ต้องการบริการกาแฟสำหรับงานประชุม Monthly 50 คน"),
                ("เอกชัย พลอยงาม",       "084-567-8901", "eakchai@store.th",     1, "สอบถามราคาเมล็ดกาแฟ Ethiopia + Colombia Blend คั่วใหม่ทุกอาทิตย์"),
                ("สิริวรรณ มั่งมี",      "085-678-9012", "siriwan@hotel.co.th",  1, "โรงแรมต้องการ Wholesale กาแฟคุณภาพดี บริการ 3 restaurant"),
                ("ปรีชา อยู่ดี",         "086-789-0123", "",                     3, "สนใจ Cupping Session สำหรับทีม 10 คน ราคาเท่าไหร่"),
                ("นงลักษณ์ ดีงาม",       "087-890-1234", "nongluk@cafe.co.th",   4, "อยากทำ Private Label สำหรับกาแฟของขวัญ แพ็คเกจสวยๆ"),
                ("ตรีวิทย์ สมบูรณ์",     "088-901-2345", "treewit@brand.com",    1, "Cafe chain 5 สาขา ต้องการ Wholesale เมล็ดกาแฟราคาพิเศษ"),
                ("กัลยา ใจงาม",          "089-012-3456", "kanlaya@biz.th",       2, "สนใจ Barista Training สำหรับพนักงานใหม่ 3 คน"),
                ("วัชรพล โชคดี",         "081-111-3333", "",                     5, "ต้องการจัดกิจกรรม Coffee Workshop สำหรับ Team Building"),
                ("อรพรรณ สว่างใจ",       "082-222-4444", "oraphan@startup.th",   3, "Startup 20 คน สนใจ Monthly Subscription กาแฟออฟฟิศ")
            };

            for (int i = 0; i < msgData.Length; i++)
            {
                var (name, phone, email, svcIdx, msg) = msgData[i];
                context.ContactMessages.Add(new ContactMessage
                {
                    FullName    = name,
                    Phone       = phone,
                    Email       = string.IsNullOrEmpty(email) ? null : email,
                    ServiceType = serviceTypes[svcIdx],
                    Message     = msg,
                    IsRead      = i < 12,
                    CreatedAt   = now.AddDays(-19 + i).AddHours(-i)
                });
            }

            // ===== Quotations (10) =====
            var qData = new[]
            {
                ("คุณสมชาย ใจดี",     "081-234-5678", "somchai@cafe.th", new[] { ("เมล็ดกาแฟ Ethiopia 1kg", 5, "กิโล", 650m), ("ถุงกาแฟ Zip 250g", 20, "ถุง", 35m) }, 0m, "paid"),
                ("บริษัท กาแฟดี จำกัด","082-345-6789", "info@kafeedi.com", new[] { ("Barista Training 1 วัน", 3, "คน", 2500m), ("อุปกรณ์ประกอบ", 1, "ชุด", 1500m) }, 5m, "accepted"),
                ("ร้านกาแฟสุชาดา",    "083-456-7890", "suchada@shop.th",  new[] { ("เมล็ดกาแฟ Colombia 5kg", 2, "กิโล", 750m), ("เมล็ดกาแฟ Blend 5kg", 3, "กิโล", 600m) }, 10m, "sent"),
                ("คุณมาลี แสงทอง",    "084-567-8901", "", new[] { ("Cold Brew ขวด 500ml", 24, "ขวด", 120m) }, 0m, "draft"),
                ("โรงแรม ชิลล์ การ์เด้น","085-678-9012","info@chillgarden.th", new[] { ("Wholesale กาแฟ 10kg/เดือน", 1, "แพ็ค", 5800m), ("อุปกรณ์ชงกาแฟ", 1, "ชุด", 3200m) }, 0m, "completed"),
                ("บริษัท สตาร์ทอัพ",  "086-789-0123", "hr@startup.co.th",  new[] { ("Subscription กาแฟ 3 เดือน", 1, "แพ็ค", 2400m) }, 0m, "paid"),
                ("คุณนิตยา เจริญ",     "087-890-1234", "nitaya@home.th",    new[] { ("Private Label 200g x 50 ถุง", 1, "ล็อต", 4500m) }, 0m, "rejected"),
                ("คาเฟ่ ไม้สัก",       "088-901-2345", "maesak@cafe.th",   new[] { ("เมล็ดกาแฟ 20kg", 1, "ล็อต", 11000m), ("บริการจัดส่ง", 1, "ครั้ง", 300m) }, 5m, "paid"),
                ("โรงเรียน อนุบาลดี",  "089-012-3456", "",                 new[] { ("Event Coffee 100 แก้ว", 1, "งาน", 4500m) }, 0m, "sent"),
                ("คุณประยุทธ์ มั่นคง",  "090-123-4567", "prayut@biz.th",   new[] { ("Cupping Session", 8, "คน", 450m) }, 0m, "draft"),
            };

            var dayOffset = -30;
            for (int i = 0; i < qData.Length; i++)
            {
                var (name, phone, email, items, discount, status) = qData[i];
                var qDate = now.AddDays(dayOffset + i * 3);
                var qNum = $"QT-{qDate:yyyyMMdd}-{(i + 1):D3}";
                var total = items.Sum(it => it.Item2 * it.Item4);
                var net = total - (total * discount / 100m);
                var isPaid = status == "paid" || status == "completed";
                var q = new Quotation
                {
                    QuotationNumber = qNum,
                    CustomerName    = name,
                    Phone           = phone,
                    Email           = string.IsNullOrEmpty(email) ? null : email,
                    DiscountPercent = discount,
                    TotalAmount     = total,
                    NetAmount       = net,
                    Status          = status,
                    PaymentStatus   = isPaid ? "paid" : status == "accepted" ? "partial" : "unpaid",
                    PaidAmount      = isPaid ? net : status == "accepted" ? net / 2 : 0,
                    PaymentMethod   = isPaid ? "โอนเงิน" : null,
                    PaidAt          = isPaid ? qDate.AddDays(2) : null,
                    CreatedAt       = qDate,
                    UpdatedAt       = qDate.AddDays(1),
                    Items           = items.Select(it => new QuotationItem
                    {
                        Description = it.Item1,
                        Quantity    = it.Item2,
                        Unit        = it.Item3,
                        UnitPrice   = it.Item4,
                        Amount      = it.Item2 * it.Item4
                    }).ToList()
                };
                context.Quotations.Add(q);
            }

            context.SaveChanges();
        }

        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
