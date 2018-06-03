using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MentorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Stripe;

namespace MentorAPI.Controllers {
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class SubscribeController : Controller {


        [HttpPost]
        [AllowAnonymous]
        public bool Post([FromBody] JObject report) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", report.Property("username").Value.ToString());

            List<Subscription> sub = new Subscription().SearchDocument(parameters);
            if (sub.Count <= 0) {
                StripeConfiguration.SetApiKey("sk_test_p9FWyo0hC9g8y39CkRR1pnYH");

                var options = new StripeCustomerCreateOptions {
                    Email = report.Property("email").Value.ToString(),
                    SourceToken = report.Property("id").Value.ToString()
                };
                var service = new StripeCustomerService();
                StripeCustomer customer = service.Create(options);

                DoSub(customer, report);
            } else {
                updateSubscription(report);
            }



            return true;
        }

        [HttpPost("updateSubscription")]
        [AllowAnonymous]
        public bool updateSubscription([FromBody] JObject report) {
            StripeConfiguration.SetApiKey("sk_test_p9FWyo0hC9g8y39CkRR1pnYH");
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", report.Property("username").Value.ToString());

            Subscription sub = new Subscription().SearchDocument(parameters)[0];

            var service = new StripeSubscriptionService();
            StripeSubscription subscription = service.Get(sub.SubID);

            var items = new List<StripeSubscriptionItemUpdateOption> {
                new StripeSubscriptionItemUpdateOption {
                PlanId = report.Property("newPlan").Value.ToString(),
                Id = subscription.Items.Data[0].Id
                },
                };
            StripeSubscriptionUpdateOptions options;
            if (report.Property("coupon").Value.ToString().Equals(string.Empty) || (!report.Property("coupon").Value.ToString().Equals("Partner25") && !report.Property("coupon").Value.ToString().Equals("tellnpfree6") && !report.Property("coupon").Value.ToString().Equals("tellnpfree12"))) {
                options = new StripeSubscriptionUpdateOptions {
                    Items = items,
                    CancelAtPeriodEnd = false
                };
            } else {
                options = new StripeSubscriptionUpdateOptions {
                    Items = items,
                    CancelAtPeriodEnd = false,
                    CouponId = report.Property("coupon").Value.ToString()
                };
            }
             
             subscription = service.Update(sub.SubID, options);
            sub.PlanName = report.Property("newPlan").Value.ToString();
            sub.UpdateDocument(sub);

            return true;
        }

        [HttpPost("cancelSubscription")]
        [AllowAnonymous]
        public bool cancelSubscription([FromBody] JObject report) {
            StripeConfiguration.SetApiKey("sk_test_p9FWyo0hC9g8y39CkRR1pnYH");
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", report.Property("username").Value.ToString());

            Subscription sub = new Subscription().SearchDocument(parameters)[0];

            var service = new StripeSubscriptionService();
            StripeSubscription subscription = service.Cancel(sub.SubID, true);

            sub.DeleteDocument();

            return true;
        }

        [HttpPost("getSubscription")]
        [AllowAnonymous]
        public Subscription getSubscription([FromBody] JObject report) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", report.Property("username").Value.ToString());

            List<Subscription> s = new Subscription().SearchDocument(parameters);
            if(s.Count >= 1) {
                return s[0];
            } else {
                return null;
            }
        }


        private void DoSub(StripeCustomer cus, JObject report) {

            StripeConfiguration.SetApiKey("sk_test_p9FWyo0hC9g8y39CkRR1pnYH");

            var items = new List<StripeSubscriptionItemOption> {
        new StripeSubscriptionItemOption {PlanId = report.Property("newPlan").Value.ToString()}
        };
            var options = new StripeSubscriptionCreateOptions {
                Items = items,
            };
            var service = new StripeSubscriptionService();
            StripeSubscription subscription = service.Create(cus.Id, options);
            InsertSubscription(subscription,report);

        }

        private void InsertSubscription(StripeSubscription subscription, JObject report) {

            new Subscription() {
                PlanName = subscription.StripePlan.Id,
                CustID = subscription.CustomerId,
                SubID = subscription.Id,
                Username = report.Property("username").Value.ToString()
            }.InsertIntoDocument();
        }


    }
}