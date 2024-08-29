using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Positron.Controls;

public class CloseButton: Button
{
    public CloseButton()
    {
        this.Text = "x";
        this.BackgroundColor = Color.Parse("Red");
    }
}

public sealed class ProgressPanel : Grid, IDisposable
{

    public readonly CancellationToken CancelToken;
    private readonly CancellationTokenSource cancellationTokenSource;

    private Grid? parentGrid;
    private readonly ProgressBar progress;

    public double Progress { get => progress.Progress; set => progress.Progress = value; }

    public static ProgressPanel Create(string title)
    {
        return new ProgressPanel(title);
    }

    public ProgressPanel(string title)
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancelToken = cancellationTokenSource.Token;
        var grid = (Application.Current.MainPage as ContentPage)?.Content as Grid;
        grid?.Children?.Add(this);
        this.HorizontalOptions = LayoutOptions.Fill;
        this.VerticalOptions = LayoutOptions.Fill;
        this.parentGrid = grid;

        this.progress = new ProgressBar();
        this.progress.HorizontalOptions = LayoutOptions.Fill;
        this.progress.VerticalOptions = LayoutOptions.Center;
        this.progress.MinimumWidthRequest = 100;

        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = 5
        });
        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });
        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });
        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = 5
        });
        this.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });

        this.RowDefinitions.Add(new RowDefinition
        {
            Height = GridLength.Star
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = 5
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = GridLength.Auto
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = GridLength.Auto
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = 5
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = GridLength.Star
        });

        this.Children.Add(new Frame
        {
            BackgroundColor = Color.Parse("White"),
            HasShadow = true,
        }.SetGrid(column: 1, columnSpan: 4, row: 1, rowSpan: 4));

        this.Children.Add(new Label
        {
            Text = title,
        }.SetGrid(column: 2, row: 2));

        this.Children.Add(progress.SetGrid(column: 2, row: 3, columnSpan: 2));
        this.Children.Add(new CloseButton
        {
            Padding = 0,
            Command = new Command(() => Cancel()),
        }.SetGrid(row: 2, column: 3));
    }

    public void Cancel()
    {
        this.Dispatcher.DispatchAsync(async () =>
        {
            if (await Application.Current.MainPage.DisplayAlert("Cancel?", "Are you sure you want to cancel?", "Yes", "No"))
            {
                cancellationTokenSource.Cancel();
                this.Dispose();
                parentGrid = null;
            }
        });
    }

    public void Dispose()
    {
        if (parentGrid == null)
        {
            return;
        }
        MainThread.BeginInvokeOnMainThread(() => parentGrid?.Children?.Remove(this));
    }
}