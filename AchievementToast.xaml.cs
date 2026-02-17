using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace LocalAchievements
{
    public partial class AchievementToast : Window
    {
        private DispatcherTimer closeTimer;

        // 1. EL EVENTO QUE FALTA (Para que NotificationService lo use)
        public event Action<AchievementToast> OnNotificationClosed;

        // 2. EL CONSTRUCTOR QUE FALTA (4 parámetros)
        public AchievementToast(string title, string description, string iconUrl, int durationSeconds)
        {
            InitializeComponent();

            // Asignamos a los controles por nombre
            TxtTitle.Text = title;
            TxtDesc.Text = description;

            if (!string.IsNullOrEmpty(iconUrl))
            {
                try { ImgIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconUrl)); }
                catch { /* Ignorar error de imagen */ }
            }

            // Posición inicial fuera de pantalla (se ajustará luego con SetTargetTop)
            this.Left = SystemParameters.WorkArea.Right;
            this.Top = SystemParameters.WorkArea.Bottom;

            // Timer para cerrar automáticamente
            closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds > 0 ? durationSeconds : 4)
            };
            closeTimer.Tick += (s, e) => CloseWithAnimation();
            closeTimer.Start();
        }

        // 3. EL MÉTODO SetTargetTop QUE FALTA
        public void SetTargetTop(double top)
        {
            if (Math.Abs(Top - top) < 1) return;

            var anim = new DoubleAnimation
            {
                To = top,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(TopProperty, anim);
        }

        private void CloseWithAnimation()
        {
            closeTimer?.Stop();
            var fade = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            fade.Completed += (s, e) =>
            {
                // Disparamos el evento antes de cerrar para que el Servicio lo sepa
                OnNotificationClosed?.Invoke(this);
                Close();
            };
            BeginAnimation(OpacityProperty, fade);
        }

        // 4. EL EVENTO DEL RATÓN QUE FALTA (Vinculado en el XAML)
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseWithAnimation();
        }

        // Evita que la notificación robe el foco
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.ShowActivated = false;
        }
    }
}