using Database.Entites.User;
using Database.PpccDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualBasic;
using Stripe;
using Stripe.Checkout;
using StripeModule.Dtos.CheckoutSessionDto;

namespace StripeModule.StripeController;

[Route("api/v1/stripe")]
[ApiController]
public class StripeController : Controller
{

    private readonly PpccDbContext _dbContext;
    private readonly IConfiguration _configuration;
    public StripeController (PpccDbContext dbContext, IConfiguration configuration)
    {   
        _configuration = configuration;
        _dbContext = dbContext;
    }


    [HttpPost("checkout")]
    public IActionResult Checkout(CheckoutSessionDto dto) 
    {
        StripeConfiguration.ApiKey = _configuration["STRIPE_SECRET_KEY"];

        var options = new Stripe.Checkout.SessionCreateOptions
        {
        SuccessUrl = "https://example.com/success",
        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
        {
            new Stripe.Checkout.SessionLineItemOptions
            {
                Price = dto.PriceId,
                Quantity = 1,
            },
        },
        Mode = "payment",
        CustomerEmail = dto.Email
        };
        var service = new Stripe.Checkout.SessionService();

        var res = service.Create(options);

        return Ok(new 
        {
            Url = res.Url
        });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook() 
    {
        var endpointSecret = _configuration["STRIPE_WEBHOOK_SECRET"];

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            var signatureHeader = Request.Headers["Stripe-Signature"];

            stripeEvent = EventUtility.ConstructEvent(json,
                    signatureHeader, endpointSecret);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted) 
            {
                var data = stripeEvent.Data.Object as Session;

                string? email = data?.CustomerDetails.Email!;
                User? user = _dbContext.GetUserByEmail(email);
                if (user == null) return BadRequest(); 
                var priceId = data?.LineItems.Data[0].Price.Id;

                if (priceId == _configuration["STRIPE_PRICE_ID_2500"]) 
                {
                    user.Credits += 2500;
                    _dbContext.SaveChanges();
                } else if (priceId == _configuration["STRIPE_PRICE_ID_15000"]) 
                {
                    user.Credits += 15000;
                    _dbContext.SaveChanges();
                } else if (priceId == _configuration["STRIPE_PRICE_ID_45000"]) 
                {
                    user.Credits += 45000;
                    _dbContext.SaveChanges();
                }
            }
            return Ok(new {
                message = "Success"
            });
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return BadRequest();
            throw;
        }
    } 
    
}