using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Report;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Geonorge.Validator.Application.Utils
{
    public class ValidationHelpers
    {
        private static readonly Regex _dimensionsRegex = new(@"srsDimension=""(?<dimensions>(\d))""", RegexOptions.Compiled);
        private static readonly Regex _gmlNamespaceRegex = new(@"xmlns:gml=""(?<gml_ns>(.*?))""", RegexOptions.Compiled);

        public static DisposableList<InputData> GetInputData(List<IFormFile> files)
        {
            return files
                .Select(file => new InputData(file.OpenReadStream(), file.FileName, null))
                .ToDisposableList();
        }

        public static T GetValidationData<T>(DisposableList<InputData> inputData, Func<DisposableList<InputData>, T> resolver) where T : class
        {
            var dataList = inputData
                .Where(data => data.IsValid)
                .ToDisposableList();

            foreach (var data in dataList)
                data.Stream.Seek(0, SeekOrigin.Begin);

            return resolver.Invoke(dataList);
        }

        public static (string GmlNamespace, int Dimensions) GetGmlMetadata(InputData inputData)
        {
            using var memoryStream = new MemoryStream();
            inputData.Stream.CopyTo(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            inputData.Stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(memoryStream);
            var xmlString = streamReader.ReadToEnd();

            var dimensionsMatch = _dimensionsRegex.Match(xmlString);
            int dimensions = 0;

            if (dimensionsMatch.Success)
                dimensions = int.Parse(dimensionsMatch.Groups["dimensions"].Value);

            var gmlNamespaceMatch = _gmlNamespaceRegex.Match(xmlString);
            string gmlNamespace = null;

            if (gmlNamespaceMatch.Success)
                gmlNamespace = gmlNamespaceMatch.Groups["gml_ns"].Value;

            return (gmlNamespace, dimensions);
        }

        public static ValidationReport CreateValidationReport(DateTime start, string xmlNamespace, DisposableList<InputData> inputData, List<Rule> rules)
        {
            return new ValidationReport
            {
                CorrelationId = ContextCorrelator.GetValue("CorrelationId") as string,
                Namespace = xmlNamespace,
                Errors = rules
                    .Where(rule => rule.Status == Status.FAILED)
                    .SelectMany(rule => rule.Messages)
                    .Count(),
                Warnings = rules
                    .Where(rule => rule.Status == Status.WARNING)
                    .SelectMany(rule => rule.Messages)
                    .Count(),
                Rules = rules
                    .ConvertAll(rule =>
                    {
                        return new ValidationRule
                        {
                            Id = rule.Id,
                            Name = rule.Name,
                            Messages = rule.Messages
                                .Select(message =>
                                {
                                    return message switch
                                    {
                                        GmlRuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, msg.XPath, msg.GmlIds),
                                        XmlRuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, msg.XPath, null),
                                        RuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, null, null),
                                        _ => null,
                                    };
                                })
                                .Where(message => message != null)
                                .ToList(),
                            MessageType = rule.MessageType.ToString(),
                            Status = rule.Status.ToString(),
                            PreCondition = rule.PreCondition,
                            ChecklistReference = rule.ChecklistReference,
                            Description = rule.Description,
                            Source = rule.Source,
                            Documentation = rule.Documentation
                        };
                    }),
                StartTime = start,
                EndTime = DateTime.Now,
                Files = inputData
                    .Select(data => data.FileName)
                    .ToList()
            };
        }
    }
}
