using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Controls;
using System;
using Avalonia.Input;
using LoopMusicPlayer.Core;

namespace LoopMusicPlayer.Controls;

public class SeekbarControl : Control
{
    public SeekbarControl()
    {
        AffectsRender<SeekbarControl>(ProgressProperty);
        AffectsRender<SeekbarControl>(LoopStartProperty);
        AffectsRender<SeekbarControl>(LoopEndProperty);
        this.AddHandler(PointerReleasedEvent, PointerReleasedHandler, handledEventsToo: true);
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        var p = e.GetPosition(this);
        var px = Math.Clamp(p.X - 5, 0, this.Bounds.Width - 10);
        double dx = px / (this.Bounds.Width - 10);
        if (this.Player is not null)
            this.Player?.Seek((long)(this.Player.TotalSamples * dx));
    }

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<SeekbarControl, double>(
                nameof(Progress),
                defaultValue: -1);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly StyledProperty<Player?> PlayerProperty =
        AvaloniaProperty.Register<SeekbarControl, Player?>(
            nameof(Player),
            defaultValue: null);

    public Player? Player
    {
        get => GetValue(PlayerProperty);
        set => SetValue(PlayerProperty, value);
    }

    public static readonly StyledProperty<double> LoopStartProperty =
        AvaloniaProperty.Register<SeekbarControl, double>(
                nameof(LoopStart),
                defaultValue: -1);

    public double LoopStart
    {
        get => GetValue(LoopStartProperty);
        set => SetValue(LoopStartProperty, value);
    }

    public static readonly StyledProperty<double> LoopEndProperty =
        AvaloniaProperty.Register<SeekbarControl, double>(
                nameof(LoopEnd),
                defaultValue: -1);

    public double LoopEnd
    {
        get => GetValue(LoopEndProperty);
        set => SetValue(LoopEndProperty, value);
    }

    public static readonly StyledProperty<IBrush> BackgroundColorProperty =
        AvaloniaProperty.Register<SeekbarControl, IBrush>(
                nameof(BackgroundColor),
                defaultValue: Brushes.LightGray);

    public IBrush BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> ActiveColorProperty =
        AvaloniaProperty.Register<SeekbarControl, IBrush>(
                nameof(ActiveColor),
                defaultValue: Brushes.White);

    public IBrush ActiveColor
    {
        get => GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> LoopColorProperty =
        AvaloniaProperty.Register<SeekbarControl, IBrush>(
                nameof(LoopColor),
                defaultValue: Brush.Parse("#3684E4"));

    public IBrush LoopColor
    {
        get => GetValue(LoopColorProperty);
        set => SetValue(LoopColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> ProgressColorProperty =
        AvaloniaProperty.Register<SeekbarControl, IBrush>(
                nameof(ProgressColor),
                defaultValue: Brushes.Black);

    public IBrush ProgressColor
    {
        get => GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public static readonly StyledProperty<IBrush> ProgressInnerColorProperty =
        AvaloniaProperty.Register<SeekbarControl, IBrush>(
                nameof(ProgressInnerColor),
                defaultValue: Brushes.White);

    public IBrush ProgressInnerColor
    {
        get => GetValue(ProgressInnerColorProperty);
        set => SetValue(ProgressInnerColorProperty, value);
    }

    public sealed override void Render(DrawingContext context)
    {
        Rect renderRect = new Rect(Bounds.Size);
        context.FillRectangle(this.BackgroundColor, renderRect);
        Rect activeRect = new Rect(5, 5, renderRect.Width - 10, renderRect.Height - 10);
        context.FillRectangle(this.ActiveColor, activeRect);

        if(0 <= LoopStart && LoopStart <= 1 && 0 <= LoopEnd && LoopEnd <= 1)
        {
            double width = (renderRect.Width - 10) * (LoopEnd - LoopStart);
            double x = (renderRect.Width - 10) * LoopStart;
            Rect loopRect = new Rect(5 + x, 5, width, renderRect.Height - 10);
            context.FillRectangle(this.LoopColor, loopRect);
        }

        if (0 <= Progress && Progress <= 1)
        {
            double x = (renderRect.Width - 10) * Progress;
            Rect progressRect = new Rect(x, 0, 10, renderRect.Height);
            context.FillRectangle(this.ProgressColor, progressRect);
            Rect progressInRect = new Rect(x + 1, 1, 8, renderRect.Height - 2);
            context.FillRectangle(this.ProgressInnerColor, progressInRect);
        }

        base.Render(context);
    }
}
