using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.ComponentModel;

namespace Maui.BottomSheet;

[ContentProperty(nameof(Content))]
public partial class BottomSheet : ContentView
{
    public static new readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(BottomSheet),
        propertyChanged: (bindable, oldValue, newValue) => ((BottomSheet)bindable).OnContentChanged((View)oldValue, (View)newValue));

    public static readonly BindableProperty BackdropOpacityProperty = BindableProperty.Create(nameof(BackdropOpacity), typeof(float), typeof(BottomSheet), .5f,
        propertyChanged: (bindable, oldValue, newValue) => ((BottomSheet)bindable).OnBackdropOpacityChanged((float)oldValue, (float)newValue));

    public static readonly BindableProperty BackdropColorProperty = BindableProperty.Create(nameof(BackdropColor), typeof(Color), typeof(BottomSheet), Color.FromArgb("#000"),
        propertyChanged: (bindable, oldValue, newValue) => ((BottomSheet)bindable).OnBackdropColorChanged((Color)oldValue, (Color)newValue));

    public static readonly BindableProperty CloseByTappingOutsideProperty = BindableProperty.Create(nameof(OnTapped), typeof(bool), typeof(BottomSheet), false);

    public static readonly BindableProperty ThresholdProperty = BindableProperty.Create(nameof(Threshold), typeof(double), typeof(BottomSheet), 50d);

    public new View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public float BackdropOpacity
    {
        get => (float)GetValue(BackdropOpacityProperty);
        set => SetValue(BackdropOpacityProperty, value);
    }

    public Color BackdropColor
    {
        get => (Color)GetValue(BackdropColorProperty);
        set => SetValue(BackdropColorProperty, value);
    }

    public bool CloseByTappingOutside
    {
        get => (bool)GetValue(CloseByTappingOutsideProperty);
        set => SetValue(CloseByTappingOutsideProperty, value);
    }

    public double Threshold
    {
        get => (double)GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    readonly Border _contentPresenter;
    bool _backdropColorIsChanging;
    bool _backgroundColorIsChanging;
    double _minimumTranslationY = 0;
    double _appliedTotalTranslationYDiff;
    bool _isHorizontalPan;
    bool _isVerticalPan;
    bool _contentPresenterSizeIsSet;
    public BottomSheet()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        Opacity = 0;
        IsVisible = false;
        IsClippedToBounds = true;
        BackgroundColor = BackdropColor.WithAlpha(BackdropOpacity);
        AbsoluteLayout.SetLayoutBounds(this, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.SizeProportional);

        _contentPresenter = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            Padding = new Thickness(10, 25),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(15, 15, 0, 0)
            }
        };
        base.Content = _contentPresenter;

        PropertyChanged += OnPropertyChanged;

        InitGestures();
    }

    private void InitGestures()
    {
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);
    }

    private void OnTapped(object? sender, TappedEventArgs eventArgs)
    {
        if (CloseByTappingOutside)
        {
            var position = eventArgs.GetPosition(_contentPresenter);
            if (position?.Y < 0) Close();
        }
    }

    protected virtual void OnPanRunning(object? sender, PanUpdatedEventArgs eventArgs)
    {
        if (_isVerticalPan)
        {
            double totalY = eventArgs.TotalY;

            var toApplyTranslationYDiff = totalY - _appliedTotalTranslationYDiff;

            var toApplyTranslationY = _contentPresenter.TranslationY + toApplyTranslationYDiff;

            toApplyTranslationY = Math.Max(toApplyTranslationY, _minimumTranslationY);

            _contentPresenter.TranslationY = toApplyTranslationY;

            _appliedTotalTranslationYDiff += toApplyTranslationYDiff;
        }
    }

    protected virtual void OnPanCompleted(object? sender, PanUpdatedEventArgs e)
    {
        if (_isVerticalPan)
        {
            ContinueOrPreventTranslationYChanging();

            _appliedTotalTranslationYDiff = 0;
        }
    }
    public virtual void OnPanUpdated(object? sender, PanUpdatedEventArgs eventArgs)
    {
        switch (eventArgs.StatusType)
        {
            case GestureStatus.Running:
                if (!_isHorizontalPan && eventArgs.IsVertical())
                {
                    _isVerticalPan = true;
                }
                else if (!_isVerticalPan && eventArgs.IsHorizontal())
                {
                    _isHorizontalPan = true;
                }
                OnPanRunning(sender, eventArgs);
                break;

            case GestureStatus.Completed:
                OnPanCompleted(sender, eventArgs);
                _isVerticalPan = false;
                _isHorizontalPan = false;
                break;
        }
    }

    private void ContinueOrPreventTranslationYChanging()
    {
        if (_contentPresenter.TranslationY > Threshold)
        {
            Close();
        }
        else
        {
            _contentPresenter.TranslateTo(0, 0, 250, Easing.SinInOut);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == BackgroundColorProperty.PropertyName)
            OnBackgroundColorChanged();
    }

    private void OnContentChanged(View oldView, View newView)
    {
        if (newView is null) return;

        _contentPresenterSizeIsSet = false;
        _contentPresenter.Content = newView;
        newView.SizeChanged += (sender, eventArgs) =>
        {
            if (!_contentPresenterSizeIsSet)
            {
                _contentPresenter.WidthRequest = Application.Current?.MainPage?.Width ?? 0;

                if (sender is ScrollView scrollView) _contentPresenter.HeightRequest = scrollView.Content.DesiredSize.Height;
                else _contentPresenter.HeightRequest = ((View)sender).DesiredSize.Height;

                if(_contentPresenter.HeightRequest == 0)
                {
                    _contentPresenter.HeightRequest = -1;
                    return;
                }

                _contentPresenter.HeightRequest += _contentPresenter.Padding.VerticalThickness;

                var mainPageHeight = Application.Current?.MainPage?.Height;
                var expectedHeight = mainPageHeight - mainPageHeight * 20 / 100 ?? 0;
                _contentPresenter.HeightRequest = Math.Min(expectedHeight, _contentPresenter.HeightRequest);

                _contentPresenterSizeIsSet = true;
            }
        };
    }

    private void OnBackdropColorChanged(Color oldColor, Color newColor)
    {
        _backdropColorIsChanging = true;
        BackgroundColor = newColor.WithAlpha(BackdropOpacity);
    }

    private void OnBackdropOpacityChanged(float oldAmount, float newAmount)
    {
        _backdropColorIsChanging = true;
        BackgroundColor = BackgroundColor.WithAlpha(newAmount);
    }

    private void OnBackgroundColorChanged()
    {
        if (_backdropColorIsChanging)
        {
            _backdropColorIsChanging = false;
            return;
        }

        if (_backgroundColorIsChanging)
        {
            _backgroundColorIsChanging = false;
            return;
        }

        _backgroundColorIsChanging = true;
        _contentPresenter.BackgroundColor = BackgroundColor;
        BackgroundColor = BackdropColor.WithAlpha(BackdropOpacity);
    }

    public async void Open()
    {
        _contentPresenter.TranslationY = Window.Height;
        Opacity = 1;
        IsVisible = true;
        
        await _contentPresenter.TranslateTo(0, 0, 250, Easing.SinInOut);
    }

    public async void Close()
    {
        await _contentPresenter.TranslateTo(0, Window.Height, 250, Easing.SinInOut);
        Opacity = 0;
        IsVisible = false;
    }
}