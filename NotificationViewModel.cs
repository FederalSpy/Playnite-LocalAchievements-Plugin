// Archivo: NotificationViewModel.cs
// Propósito: ViewModel ligero para notificaciones (título, mensaje, icono).
// Revisado: 2026-02-04 — encabezado autoañadido.
// Nota: agregar documentación y validar propiedades públicas.
namespace LocalAchievements
{
    public class NotificationViewModel
    {
        public string Header { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
