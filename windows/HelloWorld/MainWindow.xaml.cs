using System;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HelloWorld;

public sealed partial class MainWindow : Window
{
    private const int MaxDigits = 16;

    private string _entry = "0";
    private decimal _accumulator;
    private string? _pendingOperator;
    // True when the next digit starts a fresh number instead of extending the one on display.
    private bool _startNewEntry = true;
    private bool _hasError;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    private void OnDigitClick(object sender, RoutedEventArgs e)
    {
        ClearError();
        var digit = (string)((Button)sender).Content;

        if (_startNewEntry || _entry == "0")
        {
            _entry = digit;
        }
        else if (_entry.Count(char.IsDigit) < MaxDigits)
        {
            _entry += digit;
        }

        _startNewEntry = false;
        UpdateDisplay();
    }

    private void OnDecimalClick(object sender, RoutedEventArgs e)
    {
        ClearError();

        if (_startNewEntry)
        {
            _entry = "0.";
        }
        else if (!_entry.Contains('.'))
        {
            _entry += ".";
        }

        _startNewEntry = false;
        UpdateDisplay();
    }

    private void OnOperatorClick(object sender, RoutedEventArgs e)
    {
        ClearError();

        // "2 + 3 ×" evaluates 2 + 3 first; "2 + ×" just swaps the operator.
        if (_pendingOperator != null && !_startNewEntry)
        {
            if (!ApplyPendingOperator())
            {
                return;
            }
        }
        else
        {
            _accumulator = ParseEntry();
        }

        _pendingOperator = (string)((Button)sender).Tag;
        _startNewEntry = true;
        UpdateDisplay();
    }

    private void OnEqualsClick(object sender, RoutedEventArgs e)
    {
        ClearError();

        if (_pendingOperator != null)
        {
            if (!ApplyPendingOperator())
            {
                return;
            }

            _pendingOperator = null;
            _startNewEntry = true;
        }

        UpdateDisplay();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Reset();
        UpdateDisplay();
    }

    private void OnBackspaceClick(object sender, RoutedEventArgs e)
    {
        ClearError();

        if (!_startNewEntry)
        {
            _entry = _entry[..^1];
            if (_entry.Length == 0 || _entry == "-")
            {
                _entry = "0";
            }
        }

        UpdateDisplay();
    }

    private void OnNegateClick(object sender, RoutedEventArgs e)
    {
        ClearError();

        if (_entry != "0")
        {
            _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        }

        UpdateDisplay();
    }

    private void OnPercentClick(object sender, RoutedEventArgs e)
    {
        ClearError();
        _entry = Format(ParseEntry() / 100);
        UpdateDisplay();
    }

    private bool ApplyPendingOperator()
    {
        var right = ParseEntry();

        try
        {
            _accumulator = _pendingOperator switch
            {
                "+" => _accumulator + right,
                "-" => _accumulator - right,
                "*" => _accumulator * right,
                "/" => _accumulator / right,
                _ => right,
            };
        }
        catch (DivideByZeroException)
        {
            ShowError("Cannot divide by zero");
            return false;
        }
        catch (OverflowException)
        {
            ShowError("Overflow");
            return false;
        }

        _entry = Format(_accumulator);
        return true;
    }

    private decimal ParseEntry() =>
        decimal.Parse(_entry, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static string Format(decimal value)
    {
        value = Math.Round(value, 10);
        return value == 0 ? "0" : value.ToString("0.##########", CultureInfo.InvariantCulture);
    }

    private void ShowError(string message)
    {
        Reset();
        _hasError = true;
        Display.Text = message;
    }

    private void ClearError()
    {
        if (_hasError)
        {
            Reset();
        }
    }

    private void Reset()
    {
        _entry = "0";
        _accumulator = 0;
        _pendingOperator = null;
        _startNewEntry = true;
        _hasError = false;
    }

    private void UpdateDisplay() => Display.Text = _entry;
}
