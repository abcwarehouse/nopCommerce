using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.DealTherapy.Models;
using AbcWarehouse.Plugin.Misc.DealTherapy.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using SkiaSharp;

namespace AbcWarehouse.Plugin.Misc.DealTherapy.Controllers
{
    public class DealTherapyController : BasePluginController
    {
        private readonly IDealTherapyService _dealTherapyService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPriceFormatter _priceFormatter;

        public DealTherapyController(
            IDealTherapyService dealTherapyService,
            IWorkContext workContext,
            ICustomerService customerService,
            IProductService productService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IPriceFormatter priceFormatter)
        {
            _dealTherapyService = dealTherapyService;
            _workContext = workContext;
            _customerService = customerService;
            _productService = productService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _priceFormatter = priceFormatter;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new DealTherapyModel { Questions = GetQuestions() };
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Index.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Submit(DealTherapySubmitModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isGuest = await _customerService.IsGuestAsync(customer);
            int? customerId = !isGuest ? customer.Id : null;

            var answers = (model.Answers ?? new Dictionary<string, string>())
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();

            var submissionId = await _dealTherapyService.SaveSubmissionAsync(customerId, model.Email, answers);
            return RedirectToAction("Result", new { id = submissionId });
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var submission = await _dealTherapyService.GetSubmissionAsync(id);
            if (submission == null)
                return RedirectToAction("Index");

            var answers = await _dealTherapyService.GetAnswersForSubmissionAsync(id);
            var answerDict = answers.ToDictionary(a => a.QuestionKey, a => a.AnswerValue);

            var productKey = DetermineProduct(answerDict);
            var products = GetProductResults();
            products.TryGetValue(productKey ?? string.Empty, out var product);

            DealTherapyProductPreviewModel preview = null;
            if (product != null && !string.IsNullOrEmpty(product.Sku))
            {
                var nopProduct = await _productService.GetProductBySkuAsync(product.Sku);
                if (nopProduct != null && !nopProduct.Deleted && nopProduct.Published)
                {
                    var pictures = await _pictureService.GetPicturesByProductIdAsync(nopProduct.Id, 1);
                    var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(pictures.FirstOrDefault(), 370);
                    var seName = await _urlRecordService.GetSeNameAsync(nopProduct);
                    var formattedPrice = await _priceFormatter.FormatPriceAsync(nopProduct.Price);

                    preview = new DealTherapyProductPreviewModel
                    {
                        ProductId = nopProduct.Id,
                        Name = nopProduct.Name,
                        ImageUrl = imageUrl,
                        Price = formattedPrice,
                        ProductUrl = $"/{seName}"
                    };
                }
            }

            var resultModel = new DealTherapyResultModel
            {
                Email = submission.Email,
                Product = product,
                ProductPreview = preview
            };

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Result.cshtml", resultModel);
        }

        [HttpGet]
        [AuthorizeAdmin]
        [Area(AreaNames.ADMIN)]
        public async Task<IActionResult> Submissions()
        {
            var submissions = await _dealTherapyService.GetAllSubmissionsAsync();
            var products = GetProductResults();
            var rows = new List<DealTherapySubmissionRowModel>();

            foreach (var submission in submissions)
            {
                var answers = await _dealTherapyService.GetAnswersForSubmissionAsync(submission.Id);
                var answerDict = answers.ToDictionary(a => a.QuestionKey, a => a.AnswerValue);
                var productKey = DetermineProduct(answerDict);
                products.TryGetValue(productKey ?? string.Empty, out var product);
                var branch = answerDict.GetValueOrDefault("q1", "");

                rows.Add(new DealTherapySubmissionRowModel
                {
                    Id = submission.Id,
                    CustomerId = submission.CustomerId,
                    Email = submission.Email,
                    CreatedOn = submission.CreatedOnUtc.ToString("MM/dd/yyyy h:mm tt") + " UTC",
                    ProductName = product?.Name ?? "â€”",
                    Diagnosis = product?.Diagnosis ?? "â€”",
                    Branch = string.IsNullOrEmpty(branch) ? "â€”"
                           : char.ToUpper(branch[0]) + branch.Substring(1)
                });
            }

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Submissions.cshtml", rows);
        }

        [HttpGet]
        public IActionResult Share(string productKey)
        {
            var products = GetProductResults();
            if (!products.TryGetValue(productKey, out var product))
                return RedirectToAction("Index");

            return View("~/Plugins/AbcWarehouse.Plugin.Misc.DealTherapy/Views/DealTherapy/Share.cshtml",
                new DealTherapyResultModel { Product = product });
        }

        [HttpGet]
        public IActionResult ShareImage(string productKey)
        {
            var products = GetProductResults();
            if (!products.TryGetValue(productKey, out var product))
                return NotFound();

            var png = GenerateShareImage(product);
            return File(png, "image/png");
        }

        private static byte[] GenerateShareImage(ProductResult product)
        {
            const int width = 1200;
            const int height = 630;

            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;

            // Background
            using var bgPaint = new SKPaint { Color = SKColor.Parse("#111827"), Style = SKPaintStyle.Fill };
            canvas.DrawRect(0, 0, width, height, bgPaint);

            // Red left bar + top bar
            using var redPaint = new SKPaint { Color = SKColor.Parse("#e63328"), Style = SKPaintStyle.Fill };
            canvas.DrawRect(0, 0, 10, height, redPaint);
            canvas.DrawRect(0, 0, width, 8, redPaint);

            // "DEAL THERAPY" header
            using var headerPaint = new SKPaint
            {
                Color = SKColor.Parse("#e63328"),
                TextSize = 34,
                IsAntialias = true,
                FakeBoldText = true,
                Typeface = SKTypeface.Default
            };
            canvas.DrawText("DEAL THERAPY", 50, 68, headerPaint);

            // Separator
            using var linePaint = new SKPaint { Color = SKColor.Parse("#2d3748"), Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            canvas.DrawLine(50, 88, width - 50, 88, linePaint);

            // "My Deal Therapy diagnosis:" sub-label
            using var labelPaint = new SKPaint
            {
                Color = SKColor.Parse("#9ca3af"),
                TextSize = 30,
                IsAntialias = true
            };
            canvas.DrawText("My Deal Therapy diagnosis:", 50, 175, labelPaint);

            // Diagnosis name (large, scale down if too wide)
            using var diagPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 84,
                IsAntialias = true,
                FakeBoldText = true
            };
            var diagText = product.Diagnosis;
            var diagWidth = diagPaint.MeasureText(diagText);
            if (diagWidth > width - 100)
                diagPaint.TextSize = (float)((width - 100.0) / diagWidth * 84);
            canvas.DrawText(diagText, 50, 285, diagPaint);

            // Rx symbol
            using var rxSymPaint = new SKPaint
            {
                Color = SKColor.Parse("#e63328"),
                TextSize = 56,
                IsAntialias = true,
                FakeBoldText = true
            };
            canvas.DrawText("â„ž", 50, 390, rxSymPaint);

            // Product name
            using var productPaint = new SKPaint
            {
                Color = SKColor.Parse("#e5e7eb"),
                TextSize = 48,
                IsAntialias = true
            };
            var productText = product.Name;
            var productWidth = productPaint.MeasureText(productText);
            if (productWidth > width - 130)
                productPaint.TextSize = (float)((width - 130.0) / productWidth * 48);
            canvas.DrawText(productText, 120, 390, productPaint);

            // Bottom strip
            using var stripPaint = new SKPaint { Color = SKColor.Parse("#060c17"), Style = SKPaintStyle.Fill };
            canvas.DrawRect(0, height - 90, width, 90, stripPaint);

            // CTA text in strip
            using var ctaPaint = new SKPaint
            {
                Color = SKColor.Parse("#6b7280"),
                TextSize = 26,
                IsAntialias = true
            };
            canvas.DrawText("Take your session at abcwarehouse.com/deal-therapy", 50, height - 30, ctaPaint);

            using var snapshot = surface.Snapshot();
            using var data = snapshot.Encode(SKEncodedImageFormat.Png, 95);
            return data.ToArray();
        }

        private static string DetermineProduct(Dictionary<string, string> answers)
        {
            var votes = new Dictionary<string, int>();
            string q4Product = null;

            foreach (var (key, value) in answers)
            {
                if (key == "q1" || key == "q2_appliance" || string.IsNullOrEmpty(value))
                    continue;

                votes[value] = votes.GetValueOrDefault(value, 0) + 1;

                if (key.StartsWith("q4_"))
                    q4Product = value;
            }

            if (!votes.Any()) return null;

            int maxVotes = votes.Values.Max();
            var winners = votes.Where(kvp => kvp.Value == maxVotes).Select(kvp => kvp.Key).ToList();

            if (winners.Count > 1 && q4Product != null && winners.Contains(q4Product))
                return q4Product;

            return winners.FirstOrDefault();
        }

        private static List<DealTherapyQuestion> GetQuestions() => new()
        {
            // â”€â”€ Q1: Intake (everyone) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q1",
                IntroText = "Chad, steepling his fingers:",
                Text = "\"So. What brings you to Deal Therapy today?\"",
                Options = new()
                {
                    new QuizOption { Value = "dish",      Text = "\"The dishes. There are always dishes. I see them when I close my eyes.\"",             RevealBranch = "dish" },
                    new QuizOption { Value = "laundry",   Text = "\"Laundry. It has achieved sentience and it is winning.\"",                             RevealBranch = "laundry" },
                    new QuizOption { Value = "appliance", Text = "\"Mealtimes. My kitchen and I are no longer on speaking terms.\"",                      RevealBranch = "appliance" },
                    new QuizOption { Value = "furniture", Text = "\"My living room. There's nowhere to sit and everyone judges me for it.\"",              RevealBranch = "furniture" },
                    new QuizOption { Value = "tv",        Text = "\"My TV. I'm watching life's best moments on a screen that's letting me down.\"",         RevealBranch = "tv" }
                }
            },

            // â”€â”€ DISH branch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q2_dish", Branch = "dish",
                Text = "\"When your dishwasher runs, what do you need from it?\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_dishwasher",    Text = "\"To keep up â€” there's always another load, another sticky toddler cup.\"" },
                    new QuizOption { Value = "bosch_dishwasher", Text = "\"To shut up â€” if I can hear it, it's already failed me.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q3_dish", Branch = "dish",
                Text = "\"Let's talk about your relationship with germs.\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_dishwasher",    Text = "\"I have Opinions about sanitization. I'd autoclave the forks if I could.\"" },
                    new QuizOption { Value = "bosch_dishwasher", Text = "\"Clean is clean. I just don't want to think about it again for a decade.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_dish", Branch = "dish",
                Text = "\"Pick the compliment that would make you blush:\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_dishwasher",    Text = "\"'Wow, your dishwasher handles a lot.'\"" },
                    new QuizOption { Value = "bosch_dishwasher", Text = "\"'I didn't even know you owned a dishwasher. It's soâ€¦ serene.'\"" }
                }
            },

            // â”€â”€ LAUNDRY branch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q2_laundry", Branch = "laundry",
                Text = "\"Describe your laundry square footage.\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_allinone", Text = "\"A closet. A hallway nook. A spot that legally should not hold an appliance.\"" },
                    new QuizOption { Value = "whirlpool",   Text = "\"I have a real laundry room, thank you. Door and everything.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q3_laundry", Branch = "laundry",
                Text = "\"Your honest feelings about doing laundry?\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_allinone", Text = "\"If one machine could wash AND dry so I never touch the wet pile again, I'd cry.\"" },
                    new QuizOption { Value = "whirlpool",   Text = "\"I don't need it smart. I need it to work and never break, like my grandmother's.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_laundry", Branch = "laundry",
                Text = "\"Choose your laundry personality:\"",
                Options = new()
                {
                    new QuizOption { Value = "ge_allinone", Text = "\"Tech-forward, space-savvy, and frankly a little over it.\"" },
                    new QuizOption { Value = "whirlpool",   Text = "\"Budget-minded, brand-loyal, and proudly basic.\"" }
                }
            },

            // â”€â”€ APPLIANCE branch: Cook vs. Keep fork (Q2) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q2_appliance", Branch = "appliance",
                Text = "\"Is your conflict with making the food, or keeping it?\"",
                Options = new()
                {
                    new QuizOption { Value = "cook",  Text = "\"Making it. The stove and I have unfinished business.\"",    RevealSubBranch = "cook" },
                    new QuizOption { Value = "fridge", Text = "\"Keeping it. My fridge is a Tetris board and I'm losing.\"", RevealSubBranch = "fridge" }
                }
            },

            // COOK sub-branch
            new DealTherapyQuestion
            {
                Key = "q3_cook", Branch = "appliance", SubBranch = "cook",
                Text = "\"When people come over, you're the one whoâ€¦\"",
                Options = new()
                {
                    new QuizOption { Value = "stonebake_range", Text = "\"Insists on cooking for everyone. Hosting is my love language (and my cardio).\"" },
                    new QuizOption { Value = "ge_range",        Text = "\"Is texting, stirring, and refereeing children all at once.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_cook", Branch = "appliance", SubBranch = "cook",
                Text = "\"Pick your dream feature:\"",
                Options = new()
                {
                    new QuizOption { Value = "stonebake_range", Text = "\"A stone-bake pizza setting so I never order out again.\"" },
                    new QuizOption { Value = "ge_range",        Text = "\"WiFi, no-preheat, and seeing inside without opening the door.\"" }
                }
            },

            // FRIDGE sub-branch
            new DealTherapyQuestion
            {
                Key = "q3_fridge", Branch = "appliance", SubBranch = "fridge",
                Text = "\"How big is your operation?\"",
                Options = new()
                {
                    new QuizOption { Value = "madia_fridge", Text = "\"Small. Apartment, garage backup, or just me and my condiments.\"" },
                    new QuizOption { Value = "ge_fridge",    Text = "\"Standard family. We go through milk like it's a competitive sport.\"" },
                    new QuizOption { Value = "lg_fridge",    Text = "\"Big family, big hauls, big ice expectations.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_fridge", Branch = "appliance", SubBranch = "fridge",
                Text = "\"What actually sells you?\"",
                Options = new()
                {
                    new QuizOption { Value = "madia_fridge", Text = "\"Value and energy savings. I don't need it to do tricks.\"" },
                    new QuizOption { Value = "ge_fridge",    Text = "\"Sleek, sturdy, family-sized, looks elegant in the kitchen.\"" },
                    new QuizOption { Value = "lg_fridge",    Text = "\"Four kinds of ice, knock-to-see-inside, maximum storage. Gimme the gadgets.\"" }
                }
            },

            // â”€â”€ FURNITURE branch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q2_furniture", Branch = "furniture",
                Text = "\"What's the #1 job of this furniture?\"",
                Options = new()
                {
                    new QuizOption { Value = "sofa_sleeper",  Text = "\"Save space and pull double duty â€” sleep a guest, hide my clutter.\"" },
                    new QuizOption { Value = "tvx_sectional", Text = "\"Seat the whole family without anyone fighting for a cushion.\"" },
                    new QuizOption { Value = "nat_loveseat",  Text = "\"Look expensive and pulled-together. Vibes over volume.\"" },
                    new QuizOption { Value = "loveseat",      Text = "\"Swallow me whole in comfort. I recline and I do not return.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q3_furniture", Branch = "furniture",
                Text = "\"Your ideal Friday night on it:\"",
                Options = new()
                {
                    new QuizOption { Value = "sofa_sleeper",  Text = "\"Friend crashing over, or my gaming setup dialed in.\"" },
                    new QuizOption { Value = "tvx_sectional", Text = "\"Movie-night dogpile â€” kids, snacks, chaos, room for all.\"" },
                    new QuizOption { Value = "nat_loveseat",  Text = "\"A glass of wine, looking effortlessly chic, zero clutter.\"" },
                    new QuizOption { Value = "loveseat",      Text = "\"Recliner fully back, blanket, not moving for three hours.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_furniture", Branch = "furniture",
                Text = "\"Pick the review you'd leave:\"",
                Options = new()
                {
                    new QuizOption { Value = "sofa_sleeper",  Text = "\"'Genius for small spaces â€” guests have a bed AND I got storage.'\"" },
                    new QuizOption { Value = "tvx_sectional", Text = "\"'Stylish, durable, survived three kids and a dog. Worth it.'\"" },
                    new QuizOption { Value = "nat_loveseat",  Text = "\"'Gorgeous leather, looks upscale, exactly the elegant piece I wanted.'\"" },
                    new QuizOption { Value = "loveseat",      Text = "\"'Overstuffed, reclines, best decision ever. Best. Day.'\"" }
                }
            },

            // â”€â”€ TV branch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            new DealTherapyQuestion
            {
                Key = "q2_tv", Branch = "tv",
                Text = "\"Where does this TV live, and what's it for?\"",
                Options = new()
                {
                    new QuizOption { Value = "lg_oled",           Text = "\"My living room is a movie theater and I will accept nothing less.\"" },
                    new QuizOption { Value = "tcl_gaming",        Text = "\"It's command central. Console, PC, controller permanently in hand.\"" },
                    new QuizOption { Value = "samsung_outdoor",   Text = "\"Outside â€” patio, deck, by the grill. The party's out there.\"" },
                    new QuizOption { Value = "sharp_value",       Text = "\"Honestly? I just need a great TV that doesn't cost a fortune.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q3_tv", Branch = "tv",
                Text = "\"What do you actually watch?\"",
                Options = new()
                {
                    new QuizOption { Value = "lg_oled",           Text = "\"Films, the way the director intended â€” deep blacks, every shadow.\"" },
                    new QuizOption { Value = "tcl_gaming",        Text = "\"Fast, competitive, twitchy stuff where every millisecond counts.\"" },
                    new QuizOption { Value = "samsung_outdoor",   Text = "\"The game and the cookout, in full sun, without squinting.\"" },
                    new QuizOption { Value = "sharp_value",       Text = "\"A bit of everything â€” streaming, news, the usual. Just make it look good.\"" }
                }
            },
            new DealTherapyQuestion
            {
                Key = "q4_tv", Branch = "tv",
                Text = "\"Pick the spec that makes your heart race:\"",
                Options = new()
                {
                    new QuizOption { Value = "lg_oled",           Text = "\"8.3 million self-lit pixels, perfect black, Dolby Vision &amp; Atmos.\"" },
                    new QuizOption { Value = "tcl_gaming",        Text = "\"144Hz, VRR, FreeSync, near-zero lag. Built to win.\"" },
                    new QuizOption { Value = "samsung_outdoor",   Text = "\"Weatherproof, anti-glare, bright enough to beat the sun.\"" },
                    new QuizOption { Value = "sharp_value",       Text = "\"4K QLED with streaming built in â€” and a price that doesn't sting.\"" }
                }
            }
        };

        private static Dictionary<string, ProductResult> GetProductResults() => new()
        {
            ["ge_dishwasher"] = new ProductResult
            {
                Key = "ge_dishwasher",
                Name = "GE Dishwasher",
                Diagnosis = "The Capacity Crisis",
                Verdict = "You don't have a dish problem. You have a volume problem, and frankly, a control problem about germs. That's okay. We work with what we've got.\n\nThe GE keeps up with the load you swore you'd never let pile that high. Big capacity, sanitizing power strong enough to make a germaphobe weep with relief, and the quiet confidence of a machine that has seen things.",
                SideEffects = "telling guests your forks are technically cleaner than the surgical kind.",
                Sku = ""
            },
            ["bosch_dishwasher"] = new ProductResult
            {
                Key = "bosch_dishwasher",
                Name = "Bosch Dishwasher",
                Diagnosis = "Chronic Noise Sensitivity",
                Verdict = "You flinch at sound. You crave order. You once shushed a refrigerator. I understand you completely.\n\nThe Bosch is ultra-quiet, minimalist, and built to last longer than most of your relationships. It cleans in total silence and asks for nothing. Nap-compatible. Your secret is safe â€” no one will ever hear it working.",
                SideEffects = "people assuming you do dishes by hand because they never hear the machine.",
                Sku = ""
            },
            ["ge_allinone"] = new ProductResult
            {
                Key = "ge_allinone",
                Name = "GE All-in-One Washer/Dryer",
                Diagnosis = "Spatial-Capacity Crisis",
                Verdict = "You've been asked to fit two appliances into the space of roughly one shoebox. You are not the problem. Your floor plan is the problem.\n\nThe GE washes AND dries in a single machine â€” no transfers, no wet pile, no second unit you don't have room for. Built for growing families, small spaces, pet owners, and the gloriously, unapologetically lazy.",
                SideEffects = "weeping with joy the first time you never have to move a wet load again.",
                Sku = ""
            },
            ["whirlpool"] = new ProductResult
            {
                Key = "whirlpool",
                Name = "Whirlpool Washer & Dryer",
                Diagnosis = "The No-Nonsense Traditionalist",
                Verdict = "You don't want your laundry to be 'smart.' You want it to work, the way your grandmother's did, until roughly the heat death of the universe.\n\nWhirlpool: trusted, American-made, budget-friendly, and refreshingly free of features you'd never use. It washes. It dries. That is the entire pitch, and you respect that deeply.",
                SideEffects = "becoming emotionally attached to a brand your family has trusted for three generations.",
                Sku = ""
            },
            ["stonebake_range"] = new ProductResult
            {
                Key = "stonebake_range",
                Name = "Stone-Bake Range",
                Diagnosis = "Compulsive Host Syndrome",
                Verdict = "You don't cook for yourself. You cook for an audience. Hosting is your love language and, let's be honest, your cardio.\n\nThe Stone-Bake range turns your kitchen into the place everyone wants to be â€” including a true stone-bake pizza setting so you never order out again. Family-friendly, host-ready, faintly smug. Perfect.",
                SideEffects = "judging anyone who suggests delivery.",
                Sku = ""
            },
            ["ge_range"] = new ProductResult
            {
                Key = "ge_range",
                Name = "GE Smart Range",
                Diagnosis = "Multitasking Burnout",
                Verdict = "You are stirring one pot, texting your sister, and refereeing a custody dispute over the remote. You don't need more hands. You need a smarter oven.\n\nThe GE smart range brings WiFi, no-preheat, and a built-in camera so you can watch dinner without opening the door. Tech-forward cooking for the family chef who is doing six things and finishing none of them.",
                SideEffects = "preheating the oven from your phone purely because you can.",
                Sku = ""
            },
            ["madia_fridge"] = new ProductResult
            {
                Key = "madia_fridge",
                Name = "Madia Refrigerator",
                Diagnosis = "Small-Space Survivalist",
                Verdict = "You have an apartment, a garage corner, or a personality that simply does not require a six-foot fridge. Self-aware. Respectable.\n\nThe Madia is apartment-sized, Energy-Star efficient, smudge-resistant, and built around one beautiful word: value. Perfect first fridge, perfect garage backup, zero tricks.",
                SideEffects = "bragging about your energy bill to people who didn't ask.",
                Sku = ""
            },
            ["ge_fridge"] = new ProductResult
            {
                Key = "ge_fridge",
                Name = "GE Refrigerator",
                Diagnosis = "The Classic Overflow",
                Verdict = "You are a standard family going through milk like it's a competitive sport. You don't need spectacle. You need space that looks good doing it.\n\nThe GE side-by-side is sleek, sturdy, family-sized, and elegant enough to make the kitchen look intentional. The dependable workhorse that quietly carries your whole household.",
                SideEffects = "an unreasonable sense of pride every time it's fully stocked.",
                Sku = ""
            },
            ["lg_fridge"] = new ProductResult
            {
                Key = "lg_fridge",
                Name = "LG Refrigerator",
                Diagnosis = "The Gadget-Forward Big Family",
                Verdict = "Big family, big hauls, big ice expectations. You want the fridge to do tricks, and honestly? You've earned the tricks.\n\nThe LG brings four styles of ice, knock-to-see-inside glass, counter-depth styling, and maximum storage. Built for the household that runs on bulk groceries and showing off a little.",
                SideEffects = "knocking on the fridge in front of guests for no functional reason.",
                Sku = ""
            },
            ["sofa_sleeper"] = new ProductResult
            {
                Key = "sofa_sleeper",
                Name = "Sofa Sleeper",
                Diagnosis = "The Space-Saving Host",
                Verdict = "You have guests but not guest rooms, clutter but not closets, and a square footage that demands every object earn its keep.\n\nThe Sofa Sleeper pulls double duty: sleeps a guest, hides your storage, anchors a small space, and doubles as gaming HQ. Minimalist on the outside, secretly a problem-solver on the inside â€” a lot like you.",
                SideEffects = "smug satisfaction when a guest says \"wait, this is a bed?\"",
                Sku = ""
            },
            ["tvx_sectional"] = new ProductResult
            {
                Key = "tvx_sectional",
                Name = "TVX Sectional",
                Diagnosis = "Family Sprawl",
                Verdict = "You need to seat the entire household without a cushion-based turf war. This is a logistics problem, and we solve it with square footage.\n\nThe TVX sectional is stylish, durable, and built to survive kids, dogs, and movie-night dogpiles. More room for the whole family â€” finally, enough seats that nobody has to sit on the floor pretending it's fine.",
                SideEffects = "a sudden surge in friends who want to come over for movie night.",
                Sku = ""
            },
            ["nat_loveseat"] = new ProductResult
            {
                Key = "nat_loveseat",
                Name = "Nat Love Seat",
                Diagnosis = "The Refined Minimalist",
                Verdict = "You don't want more furniture. You want better furniture. Vibes over volume. Elegance over excess. I see your aesthetic and I approve.\n\nThe Nat love seat is premium leather, upscale, and effortlessly chic â€” the elegant, stylish piece that makes the whole room look like you have your life together. (You may not. The love seat won't tell.)",
                SideEffects = "rearranging the entire room so the love seat gets the best light.",
                Sku = ""
            },
            ["loveseat"] = new ProductResult
            {
                Key = "loveseat",
                Name = "Love Seat (Reclining)",
                Diagnosis = "The Comfort Devotee",
                Verdict = "You recline, and you do not return. Comfort isn't a feature to you â€” it's the entire philosophy. There's no shame here. Only cushions.\n\nThis overstuffed reclining love seat is loaded with features and built for the long sit. Blanket, snack, full recline, three undisturbed hours. As one reviewer put it: best decision ever. Best. Day.",
                SideEffects = "missing the first 40 minutes of every movie because you got too comfortable.",
                Sku = ""
            },
            ["lg_oled"] = new ProductResult
            {
                Key = "lg_oled",
                Name = "LG OLED 77\" C5 (OLED77C5PUA)",
                Diagnosis = "Cinematic Standards Disorder",
                Verdict = "You don't watch television. You attend screenings. A washed-out picture physically pains you, and frankly, you've earned the right to be this particular.\n\nThe 77-inch LG OLED evo C5 delivers over 8.3 million self-lit pixels for perfect black, perfect color, Dolby Vision and Dolby Atmos, plus FILMMAKER MODE so you see films exactly as the director intended. It's the home theater you keep telling people you're going to build â€” except it already exists, and it's enormous.",
                SideEffects = "pausing movies to make guests appreciate the shadow detail.",
                Sku = ""
            },
            ["tcl_gaming"] = new ProductResult
            {
                Key = "tcl_gaming",
                Name = "TCL 65\" QM7L Mini LED (65QM7L)",
                Diagnosis = "Competitive Reflex Syndrome",
                Verdict = "You lost a match once because of input lag and you have never fully recovered. We're not going to fix that trauma today. We're going to fix the TV.\n\nThe 65-inch TCL QM7L runs a 144Hz refresh rate with Game Accelerator 288 VRR and FreeSync for tear-free, near-zero-lag play, backed by Mini LED brightness up to 3,000 nits and 2,100+ local dimming zones. Fast, bright, and built to win â€” so the only thing left to blame is your aim.",
                SideEffects = "blaming the TV significantly less, which may be its own emotional adjustment.",
                Sku = ""
            },
            ["samsung_outdoor"] = new ProductResult
            {
                Key = "samsung_outdoor",
                Name = "Samsung 55\" The Terrace (QN55LST7D)",
                Diagnosis = "Indoor Confinement Issues",
                Verdict = "The party is outside. You are inside, squinting at a glare-blasted screen through a window. This is no way to live, and we both know it.\n\nThe 55-inch Samsung Terrace is a 4K partial-sun outdoor TV built to handle the elements â€” weather-resistant, anti-glare, and bright enough to beat daylight. The game, the cookout, and the crowd, all in one place: the backyard, where you clearly belong.",
                SideEffects = "becoming the house everyone shows up to on game day, uninvited but welcome.",
                Sku = ""
            },
            ["sharp_value"] = new ProductResult
            {
                Key = "sharp_value",
                Name = "Sharp AQUOS 55\" 4K QLED (4TC55HP7050U)",
                Diagnosis = "Overthinking-the-Purchase Paralysis",
                Verdict = "You've read 40 reviews, opened 12 tabs, and asked three coworkers. You don't need the most expensive TV. You need permission to stop researching. Granted.\n\nThe 55-inch Sharp AQUOS is 4K QLED with Xumo streaming built right in â€” a genuinely great picture and all your apps, at a price that doesn't sting. The smart, no-drama choice for someone who just wants to watch their show tonight, not write a dissertation about it.",
                SideEffects = "an overwhelming sense of relief, and roughly 40 reclaimed browser tabs.",
                Sku = ""
            }
        };
    }
}
