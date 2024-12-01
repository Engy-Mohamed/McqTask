using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using McqTask.Models;

namespace McqTask.Helpers
{
    public static class ExtractQuestions
    {
        
        public static List<Question> ExtractQuestionsFromPdf(string pdfPath)
        {
            var questions = new List<Question>();
            string text;

            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                for (int page = 3; page <= pdfDocument.GetNumberOfPages(); page += 2)
                {
                    text = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page));
                    questions.Add(ParseQuestion(text));
                }
            }

            return questions;
        }
       
        public static Question ParseQuestion(string pageText)
        {
            var question = new Question();
            question.Options = new List<Option>();


            // Extract question using regex @"\d+\.\s(.*?)(?=\n[oO])" @"\d+\.\s(.*?)(?=\n[oO]\s)"  @"(?<=\d+\.\s)(.*?)(?=\?)"

            string questionPattern = @"^\d+\s?\.(.*?\?)";//@"^\d+\.\s(.*?)(?=\?)"
            Match questionMatch = Regex.Match(pageText, questionPattern, RegexOptions.Singleline);
            question.Text = questionMatch.Success ? questionMatch.Groups[1].Value.Trim() : "Question not found";

            // Extract options using regex  @"[oO]\s(.*?)(?=\n|$)" @"(?<=\n)[oO]\s(.*?)(?=\n|$)"


            string optionsPattern = @"(?<=\n)[oO]\s+(.*?)(?=\n[oO]\s|ANS\.)";

            MatchCollection optionsMatches = Regex.Matches(pageText, optionsPattern, RegexOptions.Singleline);

            // Extract answer using regex
            string answerPattern = @"ANS\.\s(.*)";// @"ANS\.\s(.*)";
            Match answerMatch = Regex.Match(pageText, answerPattern, RegexOptions.Singleline);
            string correctAnswer = answerMatch.Success ? answerMatch.Groups[1].Value.Trim() : "Answer not found";
            question.CorrectOptionId = -1;
            for (int i = 0; i < optionsMatches.Count; i++)
            {
                string text = optionsMatches[i].Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    question.CorrectOptionId = (text == correctAnswer) ? i : question.CorrectOptionId;
                    question.Options.Add(new Option { Text = text });
                }
             
            }

            return question;
        }
        
    }
}
