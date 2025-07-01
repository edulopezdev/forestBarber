import Swal from "sweetalert2";

export function confirmDialog({
  title = "¿Estás seguro?",
  message = "",
  confirmText = "Confirmar",
  cancelText = "Cancelar",
  icon = "warning",
} = {}) {
  return Swal.fire({
    title,
    text: message,
    icon,
    showCancelButton: true,
    confirmButtonText: confirmText,
    cancelButtonText: cancelText,
    background: "#18181b",
    color: "#fff",
    confirmButtonColor: "#dc2626",
    cancelButtonColor: "#6b7280",
    buttonsStyling: true,
    reverseButtons: true,
  });
}
