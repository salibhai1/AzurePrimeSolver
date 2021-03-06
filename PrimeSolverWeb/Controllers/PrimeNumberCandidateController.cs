﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using PrimeSolverCommon;

namespace PrimeSolverWeb.Controllers
{
    public class PrimeNumberCandidateController : Controller
    {

        private IHubContext _hub = GlobalHost.ConnectionManager.GetHubContext<PrimeHub>();
        private readonly int NUMPRIMES = 100;
        //private IHubConnectionContext<dynamic> Clients
        //{
        //    get;
        //    set;
        //}

        private readonly PrimeSolver _solver = PrimeSolver.GetPrimeSolver();
        //public PrimeNumberCandidateController()
        //{
        //    Clients = GlobalHost.ConnectionManager.GetHubContext<PrimeHub>().Clients;
        //}
        
        // GET: PrimeNumberCandidate
        public ActionResult Index()
        {
            // This code executes an unbounded query; don't do this in a production app,
            // it could return too many rows for the web app to handle. For an example
            // of paging code, see:
            // http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
            var list = _solver.Get(100, true);
            return View(list);
        }

        //// GET: PrimeNumberCandidate/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var numberCandidate = _solver.Get(id.Value);
        //    if (numberCandidate == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(numberCandidate);
        //}

        // GET: PrimeNumberCandidate/Create
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult ResultNotification(int number, bool isPrime)
        {
            _hub.Clients.All.updateResult(number, isPrime);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        public ActionResult ProgressNotification(int percent)
        {
            _hub.Clients.All.updateProgress(percent);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        //// POST: PrimeNumberCandidate/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(
        //    [Bind(Include = "Number")] PrimeCandidateViewModel primeNumber
        //    //,
        //    //HttpPostedFileBase imageFile
        //    )
        //{
        //    if (!ModelState.IsValid) return View();
        //    //primeNumber.PostedDate = DateTime.Now;
        //     _solver.SolveForPrime(primeNumber);
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        /// <summary>
        /// Test NUMPRIMES more numbers to see if they contain primes.
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateMany()
        {
            var maxPrime = _solver.GetMaxSolved();
            var startWork = new PrimeNumberWorkPackage
            {
                WorkType = WorkType.NewWork,
                NumEntries = NUMPRIMES
            };

            _solver.SolveForPrime(startWork);
            for (int primeToTest = maxPrime + 1;primeToTest <= maxPrime + NUMPRIMES; primeToTest++)
            {
                var workPackage = new PrimeNumberWorkPackage
                {
                    WorkType = WorkType.ContinueWork,
                    Number = primeToTest,
                };

                 _solver.SolveForPrime(workPackage);
                Trace.TraceInformation("Created queue message for number {0}", primeToTest);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
