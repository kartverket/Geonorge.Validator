﻿using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Report;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Utils
{
    public class ValidationHelper
    {
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
                    .ConvertAll(rule => new ValidationRule(rule)),
                StartTime = start,
                EndTime = DateTime.Now,
                Files = inputData
                    .Select(data => data.FileName)
                    .ToList()
            };
        }
    }
}
