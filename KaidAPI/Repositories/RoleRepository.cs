using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaidAPI.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ServerDbContext _context;

        public RoleRepository(ServerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            var existingRole = await _context.Roles.FindAsync(role.RoleId);
            if (existingRole == null) return false;

            existingRole.RoleName = role.RoleName;
            existingRole.RoleDescription = role.RoleDescription;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}