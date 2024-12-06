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
                   var question = ParseQuestion2(text);
                    if (question != null)
                    {
                        questions.Add(question);
                    }
                   
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
           // question.CorrectOptionId = -1;
            for (int i = 0; i < optionsMatches.Count; i++)
            {
                string text = optionsMatches[i].Groups[1].Value.Trim();
                //question.CorrectOptionId = (text == correctAnswer) ? i : -1;
                question.Options.Add(new Option { Text = text });
            }

            return question;
        }

        public static Question ParseQuestion2(string text)
        {
            // Check for Matching Questions (e.g., "ANS. Trend analysis = Forecast performance based on results")
            if (Regex.IsMatch(text, @"ANS\.\s*(.*?=.*?)", RegexOptions.Singleline))
            {
                return ParseMatchingQuestion(text);
            }
            // @"(?:ANS\.|Ans\.)\s*(?:.*?\t|.*?\d+\..*)" ?:ANS\.|Ans\.)\s*(.*?)(?=\d+\..*|\t|$)
            if (Regex.IsMatch(text, @"(?:ANS\.|Ans\.)\s*(?:.*?\t|.*?\d+\..*)", RegexOptions.Singleline))
            {
                return ParseMultipleResponseQuestion(text);
            }

            // Default to Multiple Choice Question (one correct answer)
            return ParseMultipleChoiceQuestion(text);
        }

        private static Question ParseMultipleChoiceQuestion(string text)
        {
            var question = new Question { Type = "MultipleChoice" };

            // Extract the question text
            var questionMatch = Regex.Match(text, @"^\d+\s?\.(.*?\?)", RegexOptions.Singleline);// @"^\d+\s?\.(.*?\?)"
            if (questionMatch.Success)
            {
                question.Text = questionMatch.Groups[1].Value.Trim();
            }
            else
                return null;

            // Extract options
            var options = Regex.Matches(text, @"(?<=\n)[oO]\s+(.*?)(?=\n[oO]\s|ANS\.)", RegexOptions.Singleline);//;
            question.Options = new List<Option>();
            foreach (Match option in options)
            {
                
                question.Options.Add(new Option
                {
                    Text = option.Groups[1].Value.Trim(),
                    IsCorrect = false // Default until we match the correct answer
                });
            }

            // Mark the correct answer
            var answerMatch = Regex.Match(text, @"ANS\.\s(.*)", RegexOptions.Singleline);
            if (answerMatch.Success)
            {
                var correctAnswer = answerMatch.Groups[1].Value.Trim();
                foreach (var option in question.Options)
                {
                    if (option.Text == correctAnswer)
                    {
                        option.IsCorrect = true;
                    }
                }
            }
            else
                return null;

            return question;
        }

        private static Question ParseMultipleResponseQuestion(string text)
        {
            var question = new Question { Type = "MultipleResponse" };

            // Extract the question text
            var questionMatch = Regex.Match(text, @"^\d+\s?\.(.*?\?)", RegexOptions.Singleline);
            if (questionMatch.Success)
            {
                question.Text = questionMatch.Groups[1].Value.Trim();
            }
            else
                return null;

            // Extract options
            var options = Regex.Matches(text, @"(?<=\n)[oO]\s+(.*?)(?=\n[oO]\s|ANS\.)", RegexOptions.Singleline);//;
         
            question.Options = new List<Option>();
            foreach (Match option in options)
            {
                question.Options.Add(new Option
                {
                    Text = option.Groups[1].Value.Trim(),
                    IsCorrect = false // Default until we match the correct answers
                });
            }

            // Mark the correct answers
            var answersMatch = Regex.Match(text, @"(?:ANS\.|Ans\.)\s*(.*?)(?=\d+\..*|\t|$)", RegexOptions.Singleline);
            if (answersMatch.Success)
            {
                var answersText = answersMatch.Groups[1].Value;
                var correctAnswers = Regex.Split(answersText, @"\s*(?=\d+\.)").Where(o => !string.IsNullOrWhiteSpace(o));

                foreach (var correctAnswer in correctAnswers)
                {
                    foreach (var option in question.Options)
                    {
                        if (option.Text.Trim() == correctAnswer.Trim())
                        {
                            option.IsCorrect = true;
                        }
                    }
                }
            }
            else
                return null;

            return question;
        }

        private static Question ParseMatchingQuestion(string text)
        {
            var question = new Question { Type = "Matching" };
            question.Text = "Match the scenario on the left with the action on the\r\nright.";
            question.MatchingPairs = new List<MatchingPair>();

            // Extract Matching Pairs
            var matches = Regex.Matches(text, @"(.*?)\s*=\s*(.*?)\n");
            if (matches.Count() == 0)
                return null;
            foreach (Match match in matches)
            {
                question.MatchingPairs.Add(new MatchingPair
                {
                    LeftSideText = match.Groups[1].Value.Trim(),
                    RightSideText = match.Groups[2].Value.Trim()
                });
            }
            

            return question;
        }


    }
}
