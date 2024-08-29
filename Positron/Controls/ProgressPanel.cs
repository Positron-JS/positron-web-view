#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;

namespace NeuroSpeech.Positron.Controls;

public class CloseButton: Button
{
    public CloseButton()
    {
        this.Text = "x";
        this.Padding = new Thickness(10, 10, 10, 10);
        this.TextColor = Colors.Red;
        this.BackgroundColor = Colors.Transparent;
        this.FontSize = 20;
        this.MinimumWidthRequest = 40;
        this.MinimumHeightRequest = 40;
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
        this.HorizontalOptions = LayoutOptions.Fill;
        this.VerticalOptions = LayoutOptions.Fill;

        this.progress = new ProgressBar();
        progress.ZIndex = 2;
        this.progress.ProgressColor = Colors.Blue;
        this.progress.HorizontalOptions = LayoutOptions.Fill;
        this.progress.VerticalOptions = LayoutOptions.Center;
        this.progress.MinimumWidthRequest = 300;

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
            Height = 5
        });
        this.RowDefinitions.Add(new RowDefinition
        {
            Height = GridLength.Star
        });

        var cg = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition
                {
                    Width = GridLength.Star
                },
                new ColumnDefinition { Width = 30 }
            },
            Children = {
                new Label {
                    Text = title,
                    Margin = new Thickness(10,10,10,10),
                    TextColor = Colors.Gray,
                    VerticalOptions = LayoutOptions.Center,
                },
                new CloseButton
                {
                    Padding = 0,
                    Command = new Command(() => Cancel()),
                    VerticalOptions = LayoutOptions.Center,
                }.SetGrid(column: 1),
                progress.SetGrid(row: 1, columnSpan: 2)
            }
        };

        this.Children.Add(new Frame
        {
            MinimumWidthRequest = 300,
            MinimumHeightRequest = 100,
            Content = cg
        }.SetGrid(column: 2, row: 2));

        // this.Children.Add(cg.SetGrid(column: 2, row: 2));

        var grid = (Application.Current.MainPage as ContentPage)?.Content as Grid;
        grid?.Children?.Add(this);
        this.parentGrid = grid;

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