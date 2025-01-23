using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using McqTask.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using ServiceStack;

namespace McqTask.Helpers
{
    public static class ExtractQuestions
    {
        
        public static (List<Question>, List<int> UnparsedQuestionNumbers) ExtractQuestionsFromPdf(string pdfPath)
        {
            var questions = new List<Question>();
            var unparsedQuestionNumbers = new List<int>();

            string text;

            using (var pdfReader = new PdfReader(pdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                for (int page = 3; page <= pdfDocument.GetNumberOfPages(); page += 2)
                {
                    text = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page));

                    try

                    {
                        
                        var question = ParseQuestion2(text);
                        if (question != null)
                        {
                            questions.Add(question);
                        }
                        else
                        {
                            unparsedQuestionNumbers.Add(ExtractQuestionNumber(text));
                        }
                    }
                    catch
                    {
                        unparsedQuestionNumbers.Add(ExtractQuestionNumber(text));
                    }
                }
            }

            return (questions, unparsedQuestionNumbers);
        }
       
        public static Question ParseQuestion(string pageText)
        {
            bool ISAnswerWithDot = false;
            string dot_pattern = "";
            var question = new Question();
            question.Options = new List<Option>();


            // Extract question using regex @"\d+\.\s(.*?)(?=\n[oO])" @"\d+\.\s(.*?)(?=\n[oO]\s)"  @"(?<=\d+\.\s)(.*?)(?=\?)"

            string questionPattern = @"^\d+\s?\.(.*?\?)";//@"^\d+\.\s(.*?)(?=\?)"
            Match questionMatch = Regex.Match(pageText, questionPattern, RegexOptions.Singleline);
            question.Text = questionMatch.Success ? questionMatch.Groups[1].Value.Trim() : "Question not found";

            // Extract options using regex  @"[oO]\s(.*?)(?=\n|$)" @"(?<=\n)[oO]\s(.*?)(?=\n|$)"

            if (ISAnswerWithDot)
                dot_pattern = @"\.";

            string optionsPattern = @"(?<=\n)[oO]\s+(.*?)(?=\n[oO]\s|ANS"+ dot_pattern + @")";

            MatchCollection optionsMatches = Regex.Matches(pageText, optionsPattern, RegexOptions.Singleline);

            // Extract answer using regex
            string answerPattern = @"ANS"+ dot_pattern + @"\s(.*)";// @"ANS\.\s(.*)";
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
            bool ISAnswerWithDot = false;
            string dot_pattern = "";
            if (ISAnswerWithDot)
                dot_pattern = @"\.";
            // Check for Matching Questions (e.g., "ANS. Trend analysis = Forecast performance based on results")
            if (Regex.IsMatch(text, @"(?i)ANS"+ dot_pattern + @"\s*(.*?=.*?)", RegexOptions.Singleline))
            {
                var answermatch = Regex.Match(text, @"(?i)ANS"+ dot_pattern + @"\s(.*)", RegexOptions.Singleline);// @"^\d+\s?\.(.*?\?)"
                var answer  = answermatch.Groups[1].Value.Trim();
                if(IsShortAnswerFormat(answer.Replace("\n"," ").Replace(".","")))
                    return ParseMatchingQuestion2(text, dot_pattern);
                return ParseMatchingQuestion(text, dot_pattern);
            }
 
            return ParseMultipleChoiceQuestion(text, dot_pattern);
        }

        private static Question ParseMultipleChoiceQuestion(string text,string dot_pattern)
        {
            //var question = new Question { Type = "MultipleChoice" };
            var question = new Question ();
            int NumAnswersNeeded = 1;

            // Extract the question text @"^\d+\s?\.(.*?\?)""

            var questionMatch = Regex.Match(text, @"^\d+\s?\.(.*?)(?=\n[oO]\s)", RegexOptions.Singleline);// @"^\d+\s?\.(.*?\?)"
            if (questionMatch.Success)
            {
                question.Text = questionMatch.Groups[1].Value.Trim();
            }
            else
                return null;

            var chooseMatch = Regex.Match(questionMatch.Value, @"Choose\s([\w]+)");
            if (chooseMatch.Success)
            {
                string numberInWords = chooseMatch.Groups[1].Value;
                question.Text = question.Text + "  (Choose " + numberInWords + ")";
                NumAnswersNeeded = ConvertToDigit(numberInWords);
            }
         
            // Extract options
            var options = Regex.Matches(text, @"(?:^|(?<=\n))[oO]\s+(.*?)(?=\r?\n[oO]\s|\b(?i)ANS"+ dot_pattern + @")", RegexOptions.Singleline);//;
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
            var answerMatch = Regex.Match(text, @"(?i)ANS"+ dot_pattern + @"\s(.*)", RegexOptions.Singleline);
            var correctAnswersCount = 0;

            if (answerMatch.Success)
            {
                var correctAnswer = answerMatch.Groups[1].Value.Trim();
                string NormalizeNewlines(string input) =>System.Text.RegularExpressions.Regex.Replace(input.Trim().Replace("\n", " "), @"\s+", " ").Replace("’", "'").TrimEnd('.');
                foreach (var option in question.Options)
                {
                    if (NormalizeNewlines(correctAnswer) == NormalizeNewlines(option.Text))
                    {
                        option.IsCorrect = true;
                        correctAnswersCount = 1;
                        break;
                    }
                    
                }
                if (correctAnswersCount == 0)
                {
                    foreach (var option in question.Options)
                    {
                        // Use regex to ensure exact matches with word boundaries
                        string pattern = $@"\b{Regex.Escape(NormalizeNewlines(option.Text))}\b";
                        option.IsCorrect = Regex.IsMatch(NormalizeNewlines(correctAnswer), pattern, RegexOptions.IgnoreCase);
                        correctAnswersCount = option.IsCorrect ? correctAnswersCount + 1 : correctAnswersCount;
                    }
                }
            }
            else
                return null;
            
            if (correctAnswersCount > 1)
                question.Type = "Multiple Response";
            else if (correctAnswersCount == 1)
                question.Type = "Multiple Choice";
            else
                return null;
            if (NumAnswersNeeded == correctAnswersCount)
                return question;
            else
                return null;
        }

        private static Question ParseMultipleResponseQuestion(string text, string dot_pattern)
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
            var options = Regex.Matches(text, @"(?<=\n)[oO]\s+(.*?)(?=\n[oO]\s|ANS"+ dot_pattern + @")", RegexOptions.Singleline);//;
         
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
            var answersMatch = Regex.Match(text, @"(?:ANS"+ dot_pattern + @"|Ans"+ dot_pattern + @")\s*(.*?)(?=\d+\..*|\t|$)", RegexOptions.Singleline);
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
       
        private static Question ParseMatchingQuestion(string text, string dot_pattern)
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

        private static Question ParseMatchingQuestion2(string text , string dot_pattern)
        {
   
            var questionRegex = new Regex(@"(?<Id>\d+)\.\s(?<Text>.+?)\n", RegexOptions.Singleline);
            var optionsRegex = new Regex(@"o\s(?<Left>\d+)\.\s(?<LeftText>.+?)\n|o\s(?<Right>[A-Z]+)\.\s(?<RightText>.+?)\n");
            var answersRegex = new Regex(@"(?<Left>\d+)\.\s?=\s?(?<Right>[A-Z]+)");
                
                var questionMatch = questionRegex.Match(text);
                if (!questionMatch.Success) return null;

                var question = new Question
                {
                    Id = int.Parse(questionMatch.Groups["Id"].Value),
                    Text = questionMatch.Groups["Text"].Value.Trim(),
                    Type = "Matching",
                    Options = new List<Option>(),
                    MatchingPairs = new List<MatchingPair>()
                };

                // Match the options (both left and right)
                var optionMatches = optionsRegex.Matches(text);
                var leftSide = new Dictionary<string, string>();
                var rightSide = new Dictionary<string, string>();

                foreach (Match match in optionMatches)
                {
                    if (match.Groups["Left"].Success)
                    {
                        leftSide[match.Groups["Left"].Value] = match.Groups["LeftText"].Value.Trim();
                    }
                    else if (match.Groups["Right"].Success)
                    {
                        rightSide[match.Groups["Right"].Value] = match.Groups["RightText"].Value.Trim();
                    }
                }

                // Match the answers
                var answerMatches = answersRegex.Matches(text);
                foreach (Match answerMatch in answerMatches)
                {
                    var leftId = answerMatch.Groups["Left"].Value;
                    var rightId = answerMatch.Groups["Right"].Value;

                    if (leftSide.ContainsKey(leftId) && rightSide.ContainsKey(rightId))
                    {
                        question.MatchingPairs.Add(new MatchingPair
                        {
                            LeftSideText = leftSide[leftId],
                            RightSideText = rightSide[rightId]
                        });
                    }
                }

          
            return question;
        }
        public static int ConvertToDigit(string number)
        {
            number = number.ToLower();
            if (number == "two")
                return 2;
            else if (number == "three")
                return 3;
            else if (number == "four")
                return 4;
            else
                return 1;

        }
        private static int ExtractQuestionNumber(string questionText)
        {
            var match = Regex.Match(questionText, @"^\d+");
            if (match.Success && int.TryParse(match.Value, out int number))
            {
                return number;
            }
            return -1; // Use -1 for questions where the number couldn't be extracted
        }
        private static bool IsShortAnswerFormat(string input)
        {
            var answerRegex = new Regex(@"^(\d+)\s?=\s?([A-Z])$");
            // Split the input by spaces or newlines
            var entries = input.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if all entries match the format
            return entries.All(entry => answerRegex.IsMatch(entry));
        }
    }
}
