USE [ExamContext]
GO


update [dbo].[Exams] set IsActive=1, [ExamTime] =10 where  [Id]  =1


delete from Questions where Id>5