﻿namespace HabitTracker
{
    internal class Driver
    {
        public static void Main(string[] args)
        {
            HabitService.Init();

            MainMenu();
        }

        static void MainMenu()
        {
            ConsoleUtilities.Menu mainMenu = new ConsoleUtilities.Menu("Main Menu. Please select an option");

            mainMenu.AddOption("A", "All Habits...", () => { HabitsMenu(); });
            mainMenu.AddOption("N", "New Habit", () => { NewHabit(); });
            mainMenu.AddOption("Q", "Quit Program", () => { Environment.Exit(0); });

            mainMenu.SelectOption();
        }

        static void HabitsMenu()
        {
            ConsoleUtilities.Menu habitsMenu = new ConsoleUtilities.Menu("Please select a habit");

            List<HabitTable> habits = HabitService.GetAllHabits();

            int num = 1;

            habits.ForEach(habit =>
            {
                habitsMenu.AddOption((num++).ToString(), habit.HabitName, () => { HabitMenu(habit); });
            });

            habitsMenu.AddOption("B", "Go Back To Main Menu...", () => { MainMenu(); });

            habitsMenu.SelectOption();
        }

        static void NewHabit()
        {
            ConsoleUtilities.Form newHabit = new ConsoleUtilities.Form((values) =>
            {
                HabitService.InsertHabit(values[0].ToString(), values[1].ToString());
            });

            newHabit.AddStringQuery("Enter the name of the new habit");
            newHabit.AddStringQuery("Enter the units of this record");

            newHabit.Start();

            MainMenu();
        }

        static void HabitMenu(HabitTable habitTable)
        {
            ConsoleUtilities.Menu habitMenu = new ConsoleUtilities.Menu($"{habitTable.HabitName} Habit Tracker Menu. Please select an option");

            habitMenu.AddOption("A", "View All Records...", () => { HabitMenuAllRecords(habitTable); });
            habitMenu.AddOption("N", "New Record", () => { HabitMenuNewRecord(habitTable); });
            habitMenu.AddOption("D", "Delete This Habit", () => {
                ConsoleUtilities.Form confirmationForm = new ConsoleUtilities.Form((values) =>
                {
                    if (values[0].ToString().ToLower() == "y")
                    {
                        HabitService.Delete(habitTable);
                        HabitsMenu();
                    }
                });

                confirmationForm.AddStringQuery("Are you sure you want to delete this habit? (y/n)");

                confirmationForm.Start();
            });
            habitMenu.AddOption("S", "Statistics For This Habit...", () => { StatisticsMenu(habitTable); });
            habitMenu.AddOption("B", "Go Back To Main Menu...", () => { MainMenu(); });

            habitMenu.SelectOption();
        }

        static void HabitMenuAllRecords(HabitTable habitTable)
        {
            ConsoleUtilities.Menu allRecords = new ConsoleUtilities.Menu($"All records for habit: {habitTable.HabitName}");
            int num = 1;

            habitTable.GetAllRecords().ForEach((record) =>
            {
                allRecords.AddOption((num++).ToString(), $"{record.Date.Date.ToString("yyyy-MM-dd")} - {record.Value} {habitTable.TableUnit}", () => { HabitRecordMenu(habitTable, record); });
            });

            allRecords.AddOption("B", "Go Back This Habits Menu...", () => { HabitMenu(habitTable); });

            allRecords.SelectOption();
        }

        static void HabitMenuNewRecord(HabitTable habitTable)
        {
            ConsoleUtilities.Form newRecord = new ConsoleUtilities.Form((values) =>
            {
                habitTable.Insert((DateTime)values[0], values[1].ToString());
                HabitMenuAllRecords(habitTable);
            });

            newRecord.AddDateTimeQuery("Enter the date of the new record", "yyyy-MM-dd");
            newRecord.AddStringQuery($"Enter the value ({habitTable.TableUnit})");

            newRecord.Start();
        }

        static void HabitRecordMenu(HabitTable habitTable, HabitRecord habitRecord)
        {
            ConsoleUtilities.Menu recordMenu = new ConsoleUtilities.Menu($"{habitTable.HabitName.ToUpper()} Menu for Record {habitRecord.Date.Date.ToString("yyyy-MM-dd")} : {habitRecord.Value} {habitTable.TableUnit}");

            recordMenu.AddOption("U", "Update This Record", () => {
                ConsoleUtilities.Form confirmationForm = new ConsoleUtilities.Form((values) =>
                {
                    if (values[0].ToString().ToLower() == "y")
                    {
                        habitRecord.Date = (DateTime)values[1];
                        habitRecord.Value = values[2].ToString();

                        habitTable.Update(habitRecord);
                        HabitRecordMenu(habitTable, habitRecord);
                    }
                });

                confirmationForm.AddStringQuery("Are you sure you want to update this record? (y/n)");
                confirmationForm.AddDateTimeQuery("Please enter a new date for this record", "yyyy-MM-dd");
                confirmationForm.AddStringQuery($"Please enter a new value for this record ({habitTable.TableUnit})");

                confirmationForm.Start();
            });
            recordMenu.AddOption("D", "Delete This Record", () => {
                ConsoleUtilities.Form confirmationForm = new ConsoleUtilities.Form((values) =>
                {
                    if (values[0].ToString().ToLower() == "y")
                    {
                        habitTable.Delete(habitRecord);
                        HabitMenuAllRecords(habitTable);
                    }
                });

                confirmationForm.AddStringQuery("Are you sure you want to delete this record? (y/n)");

                confirmationForm.Start();
            });
            recordMenu.AddOption("B", "Go Back To All Records Menu...", () => { HabitMenuAllRecords(habitTable); });

            recordMenu.SelectOption();
        }

        static void StatisticsMenu(HabitTable habitTable)
        {
            ConsoleUtilities.Menu statsMenu = new ConsoleUtilities.Menu($"Statistics Menu for {habitTable.HabitName.ToUpper()}");

            statsMenu.AddOption("R", "Select All Records Between Dates", () =>
            {
                ConsoleUtilities.Form datesForm = new ConsoleUtilities.Form((values) =>
                {
                    List<HabitRecord> results = Statistics.RecordBetweenDates(habitTable, (DateTime)values[0], (DateTime)values[1]);

                    RecordsBetweenDatesMenu(habitTable, results, (DateTime)values[0], (DateTime)values[1]);
                });

                datesForm.AddDateTimeQuery("Enter the start date", "yyyy-MM-dd");
                datesForm.AddDateTimeQuery("Enter the end date", "yyyy-MM-dd");
                
                datesForm.Start();
            });
            statsMenu.AddOption("S", "Calculate Sum Of Values Between Dates", () =>
            {
                ConsoleUtilities.Form datesForm = new ConsoleUtilities.Form((values) =>
                {
                    float sum = Statistics.SumOfValuesBetweenDates(habitTable, (DateTime)values[0], (DateTime)values[1]);

                    Console.WriteLine($"Sum of values in records between {((DateTime)values[0]).Date.ToString("yyyy-MM-dd")} and {((DateTime)values[1]).Date.ToString("yyyy-MM-dd")}: {sum} {habitTable.TableUnit}");

                    Console.ReadLine();

                    StatisticsMenu(habitTable);
                });

                datesForm.AddDateTimeQuery("Enter the start date", "yyyy-MM-dd");
                datesForm.AddDateTimeQuery("Enter the end date", "yyyy-MM-dd");

                datesForm.Start();
            });
            statsMenu.AddOption("A", "Calculate Sum Of All Records Values", () =>
            {
                Console.WriteLine($"Sum of all values in records: {Statistics.SumOfValuesBetweenDates(habitTable)} {habitTable.TableUnit}");

                Console.ReadLine();

                StatisticsMenu(habitTable);
            });
            statsMenu.AddOption("B", "Go Back To This Habits Menu...", () => { HabitMenu(habitTable); });

            statsMenu.SelectOption();
        }

        static void RecordsBetweenDatesMenu(HabitTable habitTable, List<HabitRecord> results, DateTime start, DateTime end)
        {
            ConsoleUtilities.Menu resultsMenu = new ConsoleUtilities.Menu($"All records in {habitTable.HabitName} between {start.Date.ToString("yyyy-MM-dd")} and {end.Date.ToString("yyyy-MM-dd")}");
            int num = 1;

            results.ForEach(record =>
            {
                resultsMenu.AddOption((num++).ToString(), $"{record.Date.Date.ToString("yyyy-MM-dd")} - {record.Value} {habitTable.TableUnit}", () => { HabitRecordMenu(habitTable, record); });
            });

            resultsMenu.AddOption("B", "Go Back To Statistics Menu...", () => { StatisticsMenu(habitTable); });

            resultsMenu.SelectOption();
        }
    }
}
