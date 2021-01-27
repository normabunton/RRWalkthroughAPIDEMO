using GeneralStoreAPI_Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GeneralStoreAPI_Demo.Controllers
{
    public class TransactionController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        //POST
        //we want to remove the number of products
        //tracking inventory
        [HttpPost]
        public IHttpActionResult Post([FromBody]Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(transaction is null)
            {
                return BadRequest("Request Body Cannot be Empyt");
            }

            Customer customer = _context.Customers.Find(transaction.CustomerId);
            Product product = _context.Products.Find(transaction.Product);
            if(customer == null )
            {
                return BadRequest("Customer not found");
            }
            if(product == null)
            {
                return BadRequest("Product not found");
            }
            if (product.NumberInInventory < transaction.ItemCount)
            {
                return BadRequest("Not enough in inventory");
            }


            _context.Transactions.Add(transaction);
            transaction.Product.NumberInInventory -= transaction.ItemCount;

            _context.SaveChanges();
            return Ok("Transaction Added");

        }
        //GET
        [HttpGet]
        public IHttpActionResult GetAllTransactions()
        {
            return Ok(_context.Transactions.ToList());
        }

        //GET BY TRANSACTION ID
        [HttpGet]
        public IHttpActionResult GetByTransactionId(int id)
        {
            Transaction transaction = _context.Transactions.Find(id);
            if(transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        //GET BY CUSTOMER ID
        [HttpGet]
        [Route("api/Transaction/GetByCustomerId/{Id}")]
        public IHttpActionResult GetByCustomerId(int id)
        {
            List<Transaction> transactions = _context.Transactions.Where(t => t.CustomerId == id).ToList();
            if(transactions.Count > 0)
                return Ok(transactions);

            return BadRequest("Customer has no transactions");
        }

        //PUT(update)
        [HttpPut]
        public IHttpActionResult UpdateTransaction([FromUri]int id, [FromBody]Transaction updatedTransaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }
            if(updatedTransaction == null)
            {
                return BadRequest("Body cannot be Empty");
            }
            Transaction oldTransaction = _context.Transactions.Find(id);
            Customer newCustomer = _context.Customers.Find(updatedTransaction.CustomerId);
            Product newProduct = _context.Products.Find(updatedTransaction.Product);

            if(newCustomer is null || newProduct is null || oldTransaction is null)
            {
                return NotFound();
            }
            oldTransaction.Product.NumberInInventory += oldTransaction.ItemCount;

            oldTransaction.CustomerId = updatedTransaction.CustomerId;
            oldTransaction.ProductId = updatedTransaction.ProductId;
            oldTransaction.ItemCount = updatedTransaction.ItemCount;

            newProduct.NumberInInventory -= oldTransaction.ItemCount;

            int numberOfChanges = _context.SaveChanges();

            if (numberOfChanges > 0)           
            {
                return Ok("Updated Transaction");
            }
            return InternalServerError();
        }
        //DELETE
        [HttpDelete]
        public IHttpActionResult DeleteTransaction(int id)
        {
            Transaction transaction = _context.Transactions.Find(id);
            if(transaction == null)
            {
                return NotFound();
            }

            transaction.Product.NumberInInventory += transaction.ItemCount;


            _context.Transactions.Remove(transaction);

            

            if(_context.SaveChanges() == 2)
            {
                return Ok("Transaction Deleted");

            }
            return InternalServerError();
        }
    }
}
