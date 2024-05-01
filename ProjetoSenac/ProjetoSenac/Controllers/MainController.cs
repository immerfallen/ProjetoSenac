using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;


namespace ProjetoSenac.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public MainController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]       
        
        public async Task<ActionResult<List<ProductDTO>>> FindAll()
        {
            var products = await _repository.FindAll();
            if (products == null) { return NotFound(); };
            return Ok(products);


        }

        [HttpGet("{id}")]        
        public async Task<ActionResult<ProductDTO>> FindById(long id)
        {
            var product = await _repository.FindById(id);
            if (product.Id <= 0) { return NotFound(); };
            return Ok(product);
        }

        [HttpPost]
        
        public async Task<ActionResult<ProductDTO>> Create([FromBody] ProductDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            var res = await  _repository.Create(product);

            return Ok(res);
        }

        [HttpPut]
        
        public async Task<ActionResult<ProductDTO>> Update([FromBody] ProductDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            var res = await _repository.Update(product);
            return Ok(res);
        }

        [HttpDelete("{id}")]
       
        public async Task<ActionResult> Delete(long id)
        {
            var status = await _repository.Delete(id);
            if (!status) { return BadRequest(); };

            return Ok(status);
        }
    }
}


public class SqlServerContext : DbContext
{
    public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}

public class Product : BaseEntity
{
    [Column("name")]
    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [Column("price")]
    [Required]
    [Range(1, 10000)]
    public decimal Price { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string Description { get; set; }

    [Column("categoty_name")]
    [StringLength(50)]
    public string CategoryName { get; set; }

    [Column("image_url")]
    [StringLength(300)]
    public string ImageUrl { get; set; }
}

public class BaseEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
}

public class ProductRepository : IProductRepository
{
    private readonly SqlServerContext _context;    
    public ProductRepository(SqlServerContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDTO>> FindAll()
    {
        List<Product> products = await _context.Products.ToListAsync();

        var dto = new ProductDTO().ConvertListEntityToDTO(products);

        return dto;
    }

    public async Task<ProductDTO> FindById(long id)
    {
        Product product = await _context.Products
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync() ?? new Product();
        return new ProductDTO().ConvertEntityToDTO(product);
    }

    public async Task<ProductDTO> Create(ProductDTO productVO)
    {
        Product? product = new ProductDTO().ConvertDTOToEntity(productVO);
        _context.Products
            .Add(product);
        await _context.SaveChangesAsync();
        return new ProductDTO().ConvertEntityToDTO(product);
    }

    public async Task<ProductDTO> Update(ProductDTO productVO)
    {
        Product? product = new ProductDTO().ConvertDTOToEntity(productVO);
        _context.Products
            .Update(product);
        await _context.SaveChangesAsync();
        return new ProductDTO().ConvertEntityToDTO(product);
    }

    public async Task<bool> Delete(long id)
    {
        try
        {
            Product product = await _context.Products
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync() ?? new Product();
            if (product.Id <= 0) return false;
            _context.Products
                .Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {

            return false;
        }
    }

}

public interface IProductRepository
{
    Task<IEnumerable<ProductDTO>> FindAll();
    Task<ProductDTO> FindById(long id);
    Task<ProductDTO> Create(ProductDTO product);
    Task<ProductDTO> Update(ProductDTO product);
    Task<bool> Delete(long id);
}

public class ProductDTO
{
    public long Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public string Description { get; set; }

    public string CategoryName { get; set; }

    public string ImageUrl { get; set; }

    public List<ProductDTO> ConvertListEntityToDTO(List<Product> entities)
    {
        var dtos = new List<ProductDTO>();

        entities.ForEach(e =>
        {
            dtos.Add(new ProductDTO
            {
                CategoryName = e.CategoryName,
                Description = e.Description,
                ImageUrl = e.ImageUrl,
                Name = e.Name,
                Price = e.Price,
                Id = e.Id
            });
        });

        return dtos;
    }

    public List<Product> ConvertListDTOToEntity(List<ProductDTO> dtos)
    {
        var entities = new List<Product>();

        dtos.ForEach(e =>
        {
            dtos.Add(new ProductDTO
            {
                CategoryName = e.CategoryName,
                Description = e.Description,
                ImageUrl = e.ImageUrl,
                Name = e.Name,
                Price = e.Price,
                Id = e.Id
            });
        });

        return entities;
    }

    public Product ConvertDTOToEntity(ProductDTO dto)
    {
        if (dto != null)
        {
            return new Product
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                Id = dto.Id,
                ImageUrl = dto.ImageUrl,
                Name = dto.Name,
                Price = dto.Price,
            };
        }
        return new Product();

    }

    public ProductDTO ConvertEntityToDTO(Product entity)
    {
        if (entity != null)
        {
            return new ProductDTO
            {
                CategoryName = entity.CategoryName,
                Description = entity.Description,
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                Name = entity.Name,
                Price = entity.Price,
            };
        }
        return new ProductDTO();

    }



}


