using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace eShop.WebApp.Chatbot;

/// <summary>
/// 提供用于处理聊天消息的静态工具方法，特别是对Markdown内容的处理
/// </summary>
public static partial class MessageProcessor
{
    /// <summary>
    /// 处理消息中的Markdown图片标记，将其转换为HTML图片标签
    /// </summary>
    /// <param name="message">包含可能的Markdown图片语法的消息文本</param>
    /// <returns>处理后的带有HTML图片标签的MarkupString</returns>
    public static MarkupString AllowImages(string message)
    {
        // 处理Markdown和HTML编码不是最理想的方式。如果语言模型能以JSON等格式返回搜索结果，
        // 我们可以在.razor代码中简单地循环处理。但目前这种方式已经足够使用。

        var result = new StringBuilder(); // 用于构建最终HTML结果
        var prevEnd = 0; // 跟踪上一个匹配结束的位置

        // 替换HTML实体编码，确保能正确处理特殊字符
        message = message.Replace("&lt;", "<").Replace("&gt;", ">");

        // 遍历所有Markdown图片标记匹配项
        foreach (Match match in FindMarkdownImages().Matches(message))
        {
            // 获取当前匹配前的文本并进行HTML编码
            var contentToHere = message.Substring(prevEnd, match.Index - prevEnd);
            result.Append(HtmlEncoder.Default.Encode(contentToHere));

            // 构建HTML图片标签：将alt文本作为title属性，将URL作为src属性
            result.Append($"<img title=\"{(HtmlEncoder.Default.Encode(match.Groups[1].Value))}\" src=\"{(HtmlEncoder.Default.Encode(match.Groups[2].Value))}\" />");

            // 更新处理位置
            prevEnd = match.Index + match.Length;
        }

        // 添加最后一个匹配之后的剩余文本
        result.Append(HtmlEncoder.Default.Encode(message.Substring(prevEnd)));

        // 将结果转换为Blazor可以安全渲染的MarkupString
        return new MarkupString(result.ToString());
    }

    /// <summary>
    /// 用于匹配Markdown图片语法的正则表达式
    /// 匹配格式: ![alt text](url) 或 [alt text](url)
    /// 捕获组1: alt text (图片的替代文本)
    /// 捕获组2: url (图片的URL)
    /// </summary>
    [GeneratedRegex(@"\!?\[([^\]]+)\]\s*\(([^\)]+)\)")]
    private static partial Regex FindMarkdownImages();
}
