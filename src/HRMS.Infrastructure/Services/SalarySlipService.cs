using HRMS.Application.Interfaces;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Infrastructure.Services;

public class SalarySlipService : ISalarySlipService
{
    private readonly IUnitOfWork _unitOfWork;

    public SalarySlipService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> GeneratePdfAsync(Guid payrollRecordId)
    {
        var payroll = await _unitOfWork.Payrolls.GetByIdAsync(payrollRecordId);
        if (payroll == null) throw new NotFoundException("PayrollRecord", payrollRecordId);

        var employee = await _unitOfWork.Employees.GetWithDetailsAsync(payroll.EmployeeId);
        if (employee == null) throw new NotFoundException("Employee", payroll.EmployeeId);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("HRMS — Salary Slip").FontSize(18).Bold();
                    col.Item().AlignCenter().Text($"{GetMonthName(payroll.Month)} {payroll.Year}").FontSize(12);
                    col.Item().PaddingVertical(5).LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Employee: {employee.FirstName} {employee.LastName}").Bold();
                            c.Item().Text($"Code: {employee.EmployeeCode}");
                            c.Item().Text($"Department: {employee.Department?.Name ?? "N/A"}");
                            c.Item().Text($"Designation: {employee.Designation?.Title ?? "N/A"}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Working Days: {payroll.WorkingDays}");
                            c.Item().Text($"Days Present: {payroll.PresentDays}");
                            c.Item().Text($"Days Absent: {payroll.AbsentDays}");
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(0.5f);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("EARNINGS").Bold().FontSize(11);
                            c.Item().PaddingTop(5);
                            SlipRow(c, "Basic Salary", payroll.BasicSalary);
                            SlipRow(c, "HRA", payroll.HouseRentAllowance);
                            SlipRow(c, "Transport", payroll.TransportAllowance);
                            SlipRow(c, "Medical", payroll.MedicalAllowance);
                            SlipRow(c, "Special", payroll.SpecialAllowance);
                            if (payroll.BonusAmount > 0)
                                SlipRow(c, "Bonus", payroll.BonusAmount);
                            c.Item().PaddingTop(3).LineHorizontal(0.5f);
                            SlipRow(c, "Gross Salary", payroll.GrossSalary, true);
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DEDUCTIONS").Bold().FontSize(11);
                            c.Item().PaddingTop(5);
                            SlipRow(c, "Provident Fund", payroll.ProvidentFund);
                            SlipRow(c, "Professional Tax", payroll.ProfessionalTax);
                            SlipRow(c, "Income Tax", payroll.IncomeTax);
                            SlipRow(c, "Other Deductions", payroll.OtherDeductions);
                            if (payroll.AttendanceDeduction > 0)
                                SlipRow(c, "Attendance Deduction", payroll.AttendanceDeduction);
                            if (payroll.UnpaidLeaveDeduction > 0)
                                SlipRow(c, "Unpaid Leave", payroll.UnpaidLeaveDeduction);
                            c.Item().PaddingTop(3).LineHorizontal(0.5f);
                            SlipRow(c, "Total Deductions", payroll.TotalDeductions, true);
                        });
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text($"NET SALARY:  ₹{payroll.NetSalary:N2}")
                            .FontSize(14).Bold();
                    });
                });

                page.Footer().AlignCenter()
                    .Text($"Generated on {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC — System-generated document.")
                    .FontSize(8);
            });
        });

        return document.GeneratePdf();
    }

    private static void SlipRow(ColumnDescriptor col, string label, decimal amount, bool bold = false)
    {
        col.Item().Row(row =>
        {
            if (bold)
            {
                row.RelativeItem().Text(label).Bold();
                row.ConstantItem(100).AlignRight().Text($"₹{amount:N2}").Bold();
            }
            else
            {
                row.RelativeItem().Text(label);
                row.ConstantItem(100).AlignRight().Text($"₹{amount:N2}");
            }
        });
    }

    private static string GetMonthName(int month) => new DateTime(2000, month, 1).ToString("MMMM");
}
