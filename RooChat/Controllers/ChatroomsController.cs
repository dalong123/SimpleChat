﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RooChat.Models;

namespace RooChat.Controllers
{
    public class ChatroomsController : Controller
    {
        //
        // GET: /Chatrooms/

        public ActionResult Index()
        {
            Chatroom.RemoveOldRooms();
            return View();
        }

        public RedirectResult Create(string name, string url)
        {
            if (name == string.Empty || name == null)
                name = "RooChat";

            if (Chatroom.UrlExists(url))
            {
                TempData["error"] = "This url is already in use";
                return Redirect("/");
            }
            
            var db = new RooChatDataContext();
            var chat = new Chatroom
            {
                Name = name,
                Url = url,
                CreatedOn = DateTime.Now
            };
            if (!chat.HasValidUrl())
            {
                chat.BuildUrl();
            }
            db.Chatrooms.InsertOnSubmit(chat);
            db.SubmitChanges();
            return Redirect("/" + chat.Url);
        }

        [ActionName("View")]
        public ActionResult View_Method(string path)
        {
            if (path == "Default")
                return RedirectToAction("Index", "Chatrooms", new { id=1 });
            var chat = Chatroom.FindByUrl(path);
            return View(chat);
        }
        
        public ActionResult AddMessage(int id, string name, string message)
        {
            var db = new RooChatDataContext();
            var chat = Chatroom.Find(id);
            string return_message = string.Empty;
            if (message.Trim() != string.Empty)
            {
                var mess = new Message
                {
                    Chat_id = id,
                    Name = name,
                    Content = message,
                    SentOn = DateTime.Now,
                    Ip = "NotLogged"
                };
                db.Messages.InsertOnSubmit(mess);
                db.SubmitChanges();
                return_message = "Message Received";
            }
            else
            {
                return_message = "Ignoring Empty Message";
            }
            if (Request.IsAjaxRequest())
            {
                return new ContentResult { Content = return_message };
            }
            else
            {
                TempData["message"] = return_message;
                return Redirect("/" + chat.Url);
            }
        }

        public ViewResult FetchMessages(string chat_url, int last_m_id)
        {
            //var messages = Message.FetchFor(chat_id);
            var messages = Message.FetchAfter(last_m_id, chat_url);
            //System.Threading.Thread.Sleep(3000); //Simulate a high latency connection
            return View(messages);
        }

        public ActionResult Transcript(int id)
        {
            var chat = Chatroom.Find(id);
            Response.AddHeader("content-disposition", "attachment; filename=Transcript-" + chat.Name + ".html");
            ViewData["chat"] = chat;
            ViewData["messages"] = chat.Messages.ToList();
            return View();
        }
    }
}
