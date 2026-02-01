using HtmlAgilityPack;
using NotABot.Wrapper;
using SchrodingersBot.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Text
{
    public interface IHtmlProcessingService
    {
        Task<List<MessageObjectDTO>> PrepareHtmlForTg(string input);
    }

    public class HtmlProcessingService : IHtmlProcessingService
    {
        public HtmlProcessingService()
        {

        }

        public async Task<List<MessageObjectDTO>> PrepareHtmlForTg(string input)
        {
            List<MessageObjectDTO> result = new();
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };
            doc.LoadHtml($"{input}");

            List<MessageObjectDTO> additionalObjects = await ProcessHtmlNodeAsync(doc.DocumentNode);

            result.Add(new MessageObjectDTO()
            {
                Text = doc.DocumentNode.InnerHtml
            });

            result.AddRange(additionalObjects);
            
            return result;
        }

        private static async Task<List<MessageObjectDTO>> ProcessHtmlNodeAsync(HtmlNode node)
        {
            var additionalAnswers = new List<MessageObjectDTO>();

            List<string> BannedTags = new List<string>()
            {
                "style",
                "script"
            };

            List<string> AllowedTags = new List<string>()
            {
                "i",
                "b",
                "strong",
                "em",
                "u",
                "ins",
                "s",
                "del",
                "pre"
            };

            //Links: <a href = "https://example.com" > text </ a >
            //Mentions by user id: <a href = "tg://user?id=123456789" > name </ a >

            var children = node.ChildNodes.ToList();
            foreach (var child in children)
            {
                additionalAnswers.AddRange(await ProcessHtmlNodeAsync(child));
            }

            if (node.ParentNode != null && node.NodeType == HtmlNodeType.Element)
            {
                if (BannedTags.Contains(node.Name))
                {
                    node.ParentNode.RemoveChild(node);
                }
                else if (!AllowedTags.Contains(node.Name))
                {
                    switch (node.Name.ToLower())
                    {
                        case "img":
                            if (node.Attributes.Contains("src"))
                            {
                                var src = node.Attributes["src"].Value;
                                byte[] content = null;
                                string Id = Guid.NewGuid().ToString();
                                using (HttpClient client = new HttpClient())
                                {
                                    content = await client.GetByteArrayAsync(src);
                                }

                                additionalAnswers.Add(new MessageObjectDTO()
                                {
                                    Content = content,
                                    Text = $"[IMG:{Id}]{node.InnerText}",
                                    IsImage = true
                                });

                                node.ParentNode.ReplaceChild(
                                HtmlNode.CreateNode($"[IMG:{Id}]"),
                                node);
                            }
                            break;
                        case "a":
                            if (node.Attributes.Contains("href"))
                            {
                                var href = node.Attributes["href"];

                                node.ParentNode.ReplaceChild(
                                HtmlNode.CreateNode($"<a href=\"{href}\">{node.InnerText}</a>" ),
                                node);
                            }
                            else
                            {
                                node.ParentNode.ReplaceChild(
                                HtmlNode.CreateNode(node.InnerHtml),
                                node);
                            }
                                break;
                        default:
                            node.ParentNode.ReplaceChild(
                            HtmlNode.CreateNode(node.InnerHtml),
                            node);
                            break;
                    }


                }
                //if (node.Attributes.Any())
                //{
                //    node.Attributes.RemoveAll();
                //}
            }

            return additionalAnswers;
        }
    }
}
