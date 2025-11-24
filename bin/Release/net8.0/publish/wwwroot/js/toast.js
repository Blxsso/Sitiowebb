document.addEventListener("DOMContentLoaded", () => {
  const toast = document.getElementById("app-toast");
  if (!toast) return;

  // Cerrar con botón
  const closeBtn = toast.querySelector(".toast__close");
  const hide = () => {
    toast.classList.add("toast--hide");
    toast.addEventListener("animationend", () => toast.remove(), { once: true });
  };
  closeBtn?.addEventListener("click", hide);

  // Autocerrar a los 4.5s
  setTimeout(hide, 4500);
});
