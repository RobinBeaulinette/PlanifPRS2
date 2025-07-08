﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlanifPRS.Data;
using PlanifPRS.Models;
using System.Collections.Generic;
using System.Linq;

namespace PlanifPRS.Pages
{
    public class UsersModel : PageModel
    {
        private readonly PlanifPrsDbContext _context;

        public bool IsAdmin { get; set; }
        public List<Utilisateur> ListeUtilisateurs { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = "";

        public UsersModel(PlanifPrsDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var login = User.Identity?.Name?.Split('\\').LastOrDefault();
            var user = _context.Utilisateurs.FirstOrDefault(u => u.LoginWindows == login);

            if (user == null || user.Droits?.ToLower() != "admin")
            {
                // Redirection vers la page AccessDenied au lieu de retourner Unauthorized()
                return RedirectToPage("/AccessDenied");
            }

            IsAdmin = true;

            var query = _context.Utilisateurs.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string search = SearchTerm.ToLower();
                query = query.Where(u =>
                    (u.Nom != null && u.Nom.ToLower().Contains(search)) ||
                    (u.Prenom != null && u.Prenom.ToLower().Contains(search)) ||
                    (u.LoginWindows != null && u.LoginWindows.ToLower().Contains(search)) ||
                    (u.Mail != null && u.Mail.ToLower().Contains(search)) ||
                    (u.Service != null && u.Service.ToLower().Contains(search)) ||
                    (u.Droits != null && u.Droits.ToLower().Contains(search))
                );
            }

            ListeUtilisateurs = query.OrderBy(u => u.Nom).ThenBy(u => u.Prenom).ToList();

            // Défaut droits si null
            foreach (var util in ListeUtilisateurs)
            {
                if (string.IsNullOrEmpty(util.Droits))
                    util.Droits = "Visualiseur";
            }

            return Page();
        }

        public IActionResult OnPostUpdateDroit(int id, string nouveauDroit)
        {
            // Validation du droit
            var droitsValides = new[] { "admin", "cdp", "validateur", "process", "maintenance", "visualiseur" };
            if (!droitsValides.Contains(nouveauDroit?.ToLower()))
            {
                TempData["ErrorMessage"] = "Droit invalide sélectionné.";
                return RedirectToPage(new { searchTerm = SearchTerm });
            }

            var user = _context.Utilisateurs.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToPage(new { searchTerm = SearchTerm });
            }

            var ancienDroit = user.Droits ?? "Visualiseur";
            user.Droits = nouveauDroit;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Droits de {user.Prenom} {user.Nom} mis à jour : {ancienDroit} → {nouveauDroit}";

            // Pour garder la recherche actuelle après mise à jour
            return RedirectToPage(new { searchTerm = SearchTerm });
        }
    }
}