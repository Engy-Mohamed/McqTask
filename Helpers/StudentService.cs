using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using McqTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace McqTask.Helpers
{
    public class StudentService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(List<ApplicationUser>, List<string>)> ExtractStudentsFromExcel(string ExcelPath)
        {
            var newUsers = new List<ApplicationUser>();
            var existingUserIds = new List<string>();

            using (var workbook = new XLWorkbook(ExcelPath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                var properties = typeof(ApplicationUser).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(p => p.Name.ToLower(), p => p);

                var headers = worksheet.Row(1).Cells()
                    .Select((cell, index) => new { Name = cell.GetString().Trim().ToLower(), Index = index + 1 })
                    .Where(h => properties.ContainsKey(h.Name))
                    .ToDictionary(h => h.Name, h => h.Index);

                foreach (var row in rows)
                {
                    try
                    {
                        var user = new ApplicationUser();

                        foreach (var header in headers)
                        {
                            string columnName = header.Key;
                            int columnIndex = header.Value;
                            string cellValue = row.Cell(columnIndex).GetString().Trim();

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                var property = properties[columnName];
                                object convertedValue = Convert.ChangeType(cellValue, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                                property.SetValue(user, convertedValue);
                            }
                        }

                        var existingUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.Email == user.Email || u.PhoneNumber == user.PhoneNumber);

                        if (existingUser != null)
                        {
                            existingUserIds.Add(existingUser.Id);
                            continue;
                        }

                        newUsers.Add(user);
                    }
                    catch (Exception ex)
                    {
                        return (null, null);
                    }
                }
            }

            return (newUsers, existingUserIds);
        }
    }

}
