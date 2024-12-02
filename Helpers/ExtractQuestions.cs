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


            // Extract question using regex
            string questionPattern = @"\d+\.\s(.*?)(?=\n[oO]\s)";
            Match questionMatch = Regex.Match(pageText, questionPattern, RegexOptions.Singleline);
            question.Text = questionMatch.Success ? questionMatch.Groups[1].Value.Trim() : "Question not found";

            // Extract options using regex
            string optionsPattern = @"[oO]\s(.*?)(?=\n|$)";
            MatchCollection optionsMatches = Regex.Matches(pageText, optionsPattern, RegexOptions.Multiline);

            // Extract answer using regex
            string answerPattern = @"ANS\.\s(.*)";
            Match answerMatch = Regex.Match(pageText, answerPattern);
            string correctAnswer = answerMatch.Success ? answerMatch.Groups[1].Value.Trim() : "Answer not found";

            for (int i = 0; i < optionsMatches.Count; i++)
            {
                string text = optionsMatches[i].Groups[1].Value.Trim();

                if (text == correctAnswer)
                {
                    if (question.CorrectOptionIds == null)
                    {
                        question.CorrectOptionIds = new List<int>();
                    }
                    question.CorrectOptionIds.Add(i); // Add the correct option index
                }

                question.Options.Add(new Option { Text = text });
            }

            return question;
        }
        
    }
}
