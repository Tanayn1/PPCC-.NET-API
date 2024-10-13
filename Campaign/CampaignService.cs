using System.Text.Json;
using Campaign.Dto.CampaignDto;
using Campaign.Dto.MockUpDto;
using Campaign.Dto.Responses.CampaignGenerateResponse;
using Campaign.Dto.Responses.Extraction.BrandNameResponse;
using Campaign.Dto.Responses.Extraction.CalloutsResponse;
using Campaign.Dto.Responses.Extraction.HeadlinesResponse;
using Campaign.Dto.Responses.Extraction.Sitelinks.SitelinksResponse;
using Campaign.Dto.Responses.Extraction.Snippets.SnippetsResponse;
using Campaign.Dto.Responses.GenerateMockUpResponse;
using Campaign.Dto.Responses.MockUp;
using Campaign.Interface.ICampaignService;
using Database.PpccDbContext;
using HtmlAgilityPack;
using IronXL;
using OpenAI.Chat;
namespace Campaign.CampaignService;

public class CampaignService : ICampaignService 
{

 
    public async Task<GenerateMockUpResponse> GenerateMockup(MockUpDto dto, IConfiguration configuration) 
    {
        string scrapedContent = await ScrapeWebsite(dto.Url);
        string links = await ScrapeLinks(dto.Url);
        try
        {
            if (configuration["OPEN_AI_API_KEY"] == null) return new GenerateMockUpResponse 
            {
                Success = false,
                Message = "No Env"
            };
            ChatClient client = new(model: "gpt-4o", apiKey: configuration["OPEN_AI_API_KEY"]);
            string systemPrompt = @" You are ChatGPT, specialized in generating mockups for Google Ads. You will receive scraped data from a website, including sitelinks, headlines, and descriptions. Your task is to output the mockup data in a consistent JSON format that includes the following fields:
                                - URL of the website
                                - Display URL
                                - Headline
                                - Description
                                - Sitelinks (each with a title and description)

                                Ensure the output strictly follows this structure and format each time.

                                Sample JSON format:
                                {
                                ""url"": ""https://www.getmunch.com"",
                                ""display_url"": ""getmunch.com"",
                                ""headline"": ""GetMunch - AI Video Editing | Munch App - Register Now"",
                                ""description"": ""Extracts the most engaging, trending and impactful clips from your long-form videos, centered around machine learning capabilities, designed to keep what's important."",
                                ""sitelinks"": [
                                    {
                                    ""title"": ""GetMunch Pricing"",
                                    ""description"": ""Check Out The Pricing And Pick The Best Plan""
                                    },
                                    {
                                    ""title"": ""Login"",
                                    ""description"": ""Enter the Required Details To Log In To Your Account""
                                    },
                                    {
                                    ""title"": ""Main Page"",
                                    ""description"": ""Get To Know Us And Find Out More""
                                    },
                                    {
                                    ""title"": ""Become An Affiliate"",
                                    ""description"": ""Fill Out the Form With Your Details To Join Our Program""
                                    },
                                    {
                                    ""title"": ""Our Blog"",
                                    ""description"": ""Check Out Our Blog And Get Inspired""
                                    }
                                ]
                                }

                                Make sure that the output is concise, relevant to the provided data, and consistently formatted in JSON. Every time, the structure must follow the given format.
                                ";
            
            string userPrompt = @$"Generate a google ad mockup for this website {dto.Url} 
                        Here is content scraped from that website {scrapedContent}

                        Here are some links that were scraped from that website: {links}";

            List<ChatMessage> messages = 
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ];

            ChatCompletion chat = await client.CompleteChatAsync(messages);

            var content = chat.Content[0].Text.Trim('`', ' ', '\n');
                if (content.StartsWith("json"))
                {
                    content = content.Substring(4).Trim(); // Remove the "json" prefix
                }
            if (content == null) return new GenerateMockUpResponse 
            {
                Success = false,
                Message = "content is null"
            };  


            Console.WriteLine(content);
            MockUpResponse mockup = JsonSerializer.Deserialize<MockUpResponse>(content)!;

            return new GenerateMockUpResponse 
            {
                Success = true,
                Mockup = mockup,
                Message = "Success"
            };

        }
        catch (Exception error)
        {
        Console.WriteLine(error);
        return new GenerateMockUpResponse 
            {
                Success = false,
                Message = error.ToString()
            };              
        }
        
    }

    public async Task<CampaignGenerateResponse> CreateCampaign(CampaignDto dto, IConfiguration configuration) 
    {
        try
        {
        var scrapedContent = await ScrapeWebsite(dto.Url);
        var links = await ScrapeLinks(dto.Url);
        var brandName = await BrandName(scrapedContent, configuration);
        WorkBook wb = WorkBook.Create();
        WorkSheet sheet = wb.CreateWorkSheet("Campaign");
        sheet["A1"].Value = "Campaign";
        sheet["A2"].Value = "Campaign name";
        sheet["B2"].Value = "Ad group";
        sheet["C2"].Value = "Keyword";
        sheet["D2"].Value = "Match type";
        sheet["E2"].Value = "Max CPC";
        sheet["A3"].Value = brandName;
        sheet["B3"].Value = brandName;
        sheet["C3"].Value = brandName;
        sheet["D3"].Value = "Exact";
        sheet["E3"].Value = "0.5";

        if (dto.Headlines == true) 
        {
            sheet["A6"].Value = "Ads";
            sheet["A7"].Value = "Campaign name";
            sheet["B7"].Value = "Ad group";
            sheet["C7"].Value = "Ad type";

            for (int i = 0; i < 12; i++)
            {
                sheet.SetCellValue(7, i + 4, $"Headline {i+ 1}");
            }

            for (int i = 0; i < dto.Count; i++)
            {
                var res = await Headlines(scrapedContent, configuration);
                
                for (int j = 0; j < res?.Headlines.Length; j++)
                {
                    sheet.SetCellValue(8 + i, j + 4, res.Headlines[j]);
                }
            }
        }

        if (dto.Sitelinks) 
        {
            sheet["A12"].Value = "Sitelinks";
            sheet["A13"].Value = "Campaign name";
            sheet["B13"].Value = "Sitelink Name";
            sheet["C13"].Value = "Final URL";
            sheet["D13"].Value = "Description line 1";
            sheet["E13"].Value = "Description line 2";
            var res = await SiteLinks(scrapedContent, links, configuration);
            for (int i = 0; i < dto.Count; i++)
            {
                sheet.SetCellValue(i + 14, 1, brandName);
                sheet.SetCellValue(i + 14, 2, res.sitelink_1.name);
                sheet.SetCellValue(i + 14, 3, res.sitelink_1.final_url);
                sheet.SetCellValue(i + 14, 4, res.sitelink_1.description_1);
                sheet.SetCellValue(i + 14, 5, res.sitelink_1.description_2);

                sheet.SetCellValue(i + 15, 1, brandName);
                sheet.SetCellValue(i + 15, 2, res.sitelink_2.name);
                sheet.SetCellValue(i + 15, 3, res.sitelink_2.final_url);
                sheet.SetCellValue(i + 15, 4, res.sitelink_2.description_1);
                sheet.SetCellValue(i + 15, 5, res.sitelink_2.description_2);

                sheet.SetCellValue(i + 16, 1, brandName);
                sheet.SetCellValue(i + 16, 2, res.sitelink_3.name);
                sheet.SetCellValue(i + 16, 3, res.sitelink_3.final_url);
                sheet.SetCellValue(i + 16, 4, res.sitelink_3.description_1);
                sheet.SetCellValue(i + 16, 5, res.sitelink_3.description_2);

                sheet.SetCellValue(i + 17, 1, brandName);
                sheet.SetCellValue(i + 17, 2, res.sitelink_4.name);
                sheet.SetCellValue(i + 17, 3, res.sitelink_4.final_url);
                sheet.SetCellValue(i + 17, 4, res.sitelink_4.description_1);
                sheet.SetCellValue(i + 17, 5, res.sitelink_4.description_2);

            }
        }

        if (dto.Callouts) 
        {
            sheet["A24"].Value = "Callouts";

            sheet["A25"].Value = "Campaign Name";
            sheet["B25"].Value = "Callout Text";
            sheet["C25"].Value = "Platform Targeting";
            sheet["D25"].Value = "Platform Preference";

            var callout = await Callouts(scrapedContent, configuration);

            for (int i = 0; i < dto.Count; i++)
            {
                sheet.SetCellValue(i + 26, 1, brandName);
                sheet.SetCellValue(i + 26, 2, callout?.callout_1);
                sheet.SetCellValue(i + 26, 3, "all");
                sheet.SetCellValue(i + 26, 4, "all");

                sheet.SetCellValue(i + 27, 1, brandName);
                sheet.SetCellValue(i + 27, 2, callout?.callout_2);
                sheet.SetCellValue(i + 27, 3, "all");
                sheet.SetCellValue(i + 27, 4, "all");

                sheet.SetCellValue(i + 28, 1, brandName);
                sheet.SetCellValue(i + 28, 2, callout?.callout_3);
                sheet.SetCellValue(i + 28, 3, "all");
                sheet.SetCellValue(i + 28, 4, "all");

                sheet.SetCellValue(i + 29, 1, brandName);
                sheet.SetCellValue(i + 29, 2, callout?.callout_4);
                sheet.SetCellValue(i + 29, 3, "all");
                sheet.SetCellValue(i + 29, 4, "all");
            }

        }

        if (dto.Snippets) 
        {
            sheet["A48"].Value = "Snippets";
            sheet["A49"].Value = "Campaign Name";
            sheet["B49"].Value = "Header";
            sheet["C49"].Value = "Snippet Values";
            sheet["D49"].Value = "Platform Targeting";
            sheet["E49"].Value = "Platform Preference";

            var snippets = await Snippets(scrapedContent, configuration);

            for (int i = 0; i < dto.Count; i++)
            {
                sheet.SetCellValue(i + 50, 1, snippets?.snippet_group_1.header);
                sheet.SetCellValue(i + 50, 2, $"{snippets?.snippet_group_1.snippet_1}, {snippets?.snippet_group_1.snippet_2}, {snippets?.snippet_group_1.snippet_3}, {snippets?.snippet_group_1.snippet_4}");
                sheet.SetCellValue(i + 50, 3, "all");
                sheet.SetCellValue(i + 50, 4, "all");

                sheet.SetCellValue(i + 51, 1, snippets?.snippet_group_2.header);
                sheet.SetCellValue(i + 51, 2, $"{snippets?.snippet_group_2.snippet_1}, {snippets?.snippet_group_2.snippet_2}, {snippets?.snippet_group_2.snippet_3}, {snippets?.snippet_group_2.snippet_4}");
                sheet.SetCellValue(i + 51, 3, "all");
                sheet.SetCellValue(i + 51, 4, "all");

            }
        }


        var stream = wb.SaveAs("").ToStream();

        return new CampaignGenerateResponse 
        {
            Success = true,
            Message = "Success",
            Camapaign = stream.ToArray()
        };
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return new CampaignGenerateResponse 
            {
                Success = false,
                Message = error.ToString()
            };
            throw;
        }
       

    }

    private static async Task<HeadlinesResponse?> Headlines(string scrapedContent, IConfiguration configuration) 
    {
        try
        {
            string systemPrompt = @"
                            You are a Google Ads headline generator designed to create engaging and concise headlines that meet the following criteria:

                            Each headline must be 30 characters or less.
                            All words should start with a capital letter for readability, except for minor connecting words such as 'and,' 'for,' or 'about.'
                            Headlines should be simple, easy to understand, and tailored to drive a high click-through rate (CTR).
                            Keep the language basic and clear, prioritizing short, actionable phrases.
                            Headlines should be provided in the following JSON format:

                            {
                                ""Headlines"": [
                                    ""Headline 1"",
                                    ""Headline 2"",
                                    ""Headline 3"",
                                    ...
                                ]
                            }

                            Only generate headlines in the JSON format without additional text or comments. Each time you respond, output exactly 12 headlines.";
            

            string userPrompt = @$"
                        I need your help crafting 12 effective headlines for my Google search ads. From the scraped website content provided please ensure that each headline meets the following criteria:

                        Maximum of 30 characters per headline.
                        All words should start with a capital letter for readability, except for less meaningful connecting words like 'and,' 'for,' or 'about.'
                        Generate the headlines in a table format with 12 columns and 2 rows for easy copy-pasting.
                        Aim for a high click-through rate (CTR).
                        Keep the language simple, concise, and easy to understand.
                        Do not generate anything other than the table.
                        Remember:

                        Headlines should be engaging and relevant.
                        Prioritize clarity and call-to-action to encourage users to click.
                        Stick to the 30-character limit per headline.
                        Ensure all words follow the capitalization rule (except connecting words).

                        Here is the scraped content: {scrapedContent}
                        ";
            var content = await Extract(systemPrompt, userPrompt, configuration);

            HeadlinesResponse headlines = JsonSerializer.Deserialize<HeadlinesResponse>(content)!;

            Console.Write($"Headlines json {headlines}");

            return headlines;

        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }

    private static async Task<SnippetsResponse?> Snippets (string scrapedContent, IConfiguration configuration) 
    {
        try
        {
            string systemPrompt = @" You are a Google Ads structured snippet generator that creates structured snippets based on website content. Your output must always be in the following JSON format:

                    {
                    ""snippet_group_1"": {
                        ""header"": "",
                        ""snippet_1"": "",
                        ""snippet_2"": "",
                        ""snippet_3"": "",
                        ""snippet_4"": ""
                    },
                    ""snippet_group_2"": {
                        ""header"": "",
                        ""snippet_1"": "",
                        ""snippet_2"": "",
                        ""snippet_3"": "",
                        ""snippet_4"": ""
                    }
                    }
            

                Instructions:

                Scrape the website data and select relevant information to populate the structured snippets.
                Choose from the available headers: Amenities, Brands, Courses, Degree Programs, Destinations, Featured Hotels, Insurance Coverage, Models, Neighborhoods, Service Catalog, Shows, Styles, Types.
                Provide exactly 2 headers, each with 4 snippet values.
                The header and snippet values must be well-matched based on the website's content.
                Only return the JSON structure, no additional text.";

            string userPrompt = @$"Please assist me in writing good structured snippets for my Google search ads. I want you to write a total of 8 snippets, organized into 2 headers. When writing these, keep in mind the following:

                        Research the provided website URL and extract relevant information to create the snippets.
                        Use clear headers from the following options: Amenities, Brands, Courses, Degree Programs, Destinations, Featured Hotels, Insurance Coverage, Models, Neighborhoods, Service Catalog, Shows, Styles, Types.
                        Under each header, list 4 snippet values without any additional formatting or characters.
                        Here is the scraped website data: {scrapedContent}";
            var content = await Extract(systemPrompt, userPrompt, configuration);

            SnippetsResponse snippets = JsonSerializer.Deserialize<SnippetsResponse>(content)!;
            
            return snippets;
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }

    private static async Task<CalloutsResponse?> Callouts(string scrapedContent, IConfiguration configuration) 
    {
        try
        {
            var systemPrompt = @"You are a Google Ads callout generator designed to create concise and impactful callouts for search ads. Your output must always be in the following JSON format:
                            {
                                ""callout_1"": "",
                                ""callout_2"": "",
                                ""callout_3"": "",
                                ""callout_4"": ""
                            }

                            Instructions:

                            Generate a total of 4 callouts for a Google Ads campaign.
                            Each callout should be engaging, clear, and relevant to the products or services being advertised.
                            Focus on highlighting unique selling points or key benefits to encourage clicks.
                            Only return the JSON structure, with no additional text or comments.";

            var userPrompt = @$"Please assist me in writing effective callouts for my Google search ads campaign. I need a total of 4 callouts. When writing these, keep in mind the following:

                        Use the content extracted from the provided website content to inform your callouts.
                        Each callout should be concise, engaging, and highlight key benefits or unique selling points of the product or service.
                        Ensure the callouts are relevant to the website content and encourage clicks.
                        
                        Scraped Website Content: {scrapedContent}";

            var content = await Extract(systemPrompt, userPrompt, configuration);

            CalloutsResponse callouts = JsonSerializer.Deserialize<CalloutsResponse>(content)!;

            return callouts;

        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }

    private static async Task<SitelinksResponse> SiteLinks(string scrapedContent, string links, IConfiguration configuration) 
    {
        try
        {
            var systemPrompt = @"You are a Google Ads sitelink generator tasked with creating comprehensive sitelinks for search ads. Your output must be in the following JSON format:

                            {
                                ""sitelink_1"": {
                                    ""name"": "",
                                    ""final_url"": "",
                                    ""description_1"": "",
                                    ""description_2"": ""
                                },
                                ""sitelink_2"": {
                                    ""name"": "",
                                    ""final_url"": "",
                                    ""description_1"": "",
                                    ""description_2"": ""
                                },
                                ""sitelink_3"": {
                                    ""name"": "",
                                    ""final_url"": "",
                                    ""description_1"": "",
                                    ""description_2"": ""
                                },
                                ""sitelink_4"": {
                                    ""name"": "",
                                    ""final_url"": "",
                                    ""description_1"": "",
                                    ""description_2"": ""
                                }
                            }
                            Instructions:

                            Generate a total of 4 sitelinks for a Google Ads campaign.
                            Each sitelink must include the following:
                            Sitelink name: Use standard yet relevant names for common pages.
                            Final URL: Provide a relevant URL for each sitelink.
                            Description line 1: Ensure it is more than 7 words but not more than 12.
                            Description line 2: Ensure it is also more than 7 words but not more than 12.
                            The descriptions should effectively communicate the value of the linked pages.
                            Only return the JSON structure, with no additional text or comments.";

            var userPrompt = @$"Please assist me in writing effective sitelinks for my Google search ads campaign. I need a total of 4 sitelinks. Each sitelink should include the following:

                        Sitelink name: Use standard yet relevant names for common pages.
                        Final URL: Provide a relevant URL for each sitelink.
                        Description line 1: Ensure it has more than 7 words but not more than 12.
                        Description line 2: Ensure it also has more than 7 words but not more than 12.
                        Make sure the descriptions effectively communicate the value of the linked pages.
                        
                        Here are the urls: {links};

                        Here is scraped website content for context: {scrapedContent}";

            var content = await Extract(systemPrompt, userPrompt, configuration);

            SitelinksResponse sitelinks = JsonSerializer.Deserialize<SitelinksResponse>(content)!;

            return sitelinks;
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }

    private static async Task<BrandNameResponse?> BrandName(string scrapedContent, IConfiguration configuration) 
    {
        try
        {
            var systemPrompt = @"You are a web scraper specialized in extracting brand names from website content. Your task is to identify and return only the brand name. Respond with valid JSON in the following format:

                                    {
                                        ""brandName"": ""{{extracted brand name}}""
                                    }

                                    Ensure that your output contains only the brand name, formatted exactly as shown.";
            var userPrompt = @$"Below is the scraped content. Please extract and return only the brand name:

                                {scrapedContent}

                                Only return the brand name in your response.";

            var content = await Extract(systemPrompt, userPrompt, configuration);

            BrandNameResponse brandName = JsonSerializer.Deserialize<BrandNameResponse>(content)!;

            return brandName;
            
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }

    private static async Task<string> Extract(string systemPrompt, string userPrompt, IConfiguration configuration) 
    {
        try
        {
            ChatClient client = new(model: "gpt-4o", apiKey: configuration["OPEN_AI_API_KEY"]);
            List<ChatMessage> messages = 
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ];
            ChatCompletion chat = await client.CompleteChatAsync(messages);

            var content = chat.Content[0].Text.Trim('`', ' ', '\n');
                if (content.StartsWith("json"))
                {
                    content = content.Substring(4).Trim(); 
                }
            Console.WriteLine(content);

            return content;

        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            throw;
        }
    }   

    private static async Task<string> ScrapeWebsite(string url) 
    {
        var client = new HttpClient();
        var page = await client.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(page);
        var ptags = htmlDoc.DocumentNode.SelectNodes("//p");
        var h1tags = htmlDoc.DocumentNode.SelectNodes("//h1");
        var h2tags = htmlDoc.DocumentNode.SelectNodes("//h2");
        var h3tags = htmlDoc.DocumentNode.SelectNodes("//h3");

        string str = "";

        if (ptags != null) {
            foreach (var p in ptags)
            {
                str = p.InnerText.Trim() + str;
            }
        }

        if (h1tags != null) {
            foreach (var h1 in h1tags)
            {
                str = h1.InnerText.Trim() + str;
            }
        }

        if (h2tags != null) {
            foreach (var h2 in h2tags)
            {
                str = h2.InnerText.Trim() + str;
            }
        }


        if (h3tags != null) {
            foreach (var h3 in h3tags)
            {
                str = h3.InnerText.Trim() + str;
            }
        }

        Console.WriteLine($"The scraped content: {str}");

        return str;

    }

    private static async Task<string> ScrapeLinks(string url) 
    {
        var client = new HttpClient();
        var page = await client.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(page);

        var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        var links = "";
        if (linkNodes != null) {
            foreach (var link in linkNodes)
            {
                string href = link.GetAttributeValue("href", string.Empty);
                Console.WriteLine(href);
                links = links + "  " + href;
                
            }
        }
        Console.WriteLine($"The Links: {links}");

        return links;

    }

}