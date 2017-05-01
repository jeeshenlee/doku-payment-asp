﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Doku;
using System.Diagnostics;

namespace doku_library.example_payment
{
    public partial class merchant_example : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            string words = "";
            if (!Page.IsPostBack)
            {
                Doku_Initiate.sharedKey = "k8UhY5t4RF4e"; //local
                //Doku_Initiate.sharedKey = "aKh4dSX72d6C";//luna
                Doku_Initiate.mallId = Request.Form["doku_mall_id"];

                #region PREDATA
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("amount", Request.Form["doku_amount"]);
                param.Add("invoice", Request.Form["doku_invoice_no"]);
                param.Add("currency", Request.Form["doku_currency"]);
                param.Add("pairing_code", Request.Form["doku_pairing_code"]);
                param.Add("token", Request.Form["doku_token"]);
                words = Library.doCreateWords(param);

                List<string> basket = new List<string>();
                basket.Add("sayur");
                basket.Add("10000.00");
                basket.Add("1");
                basket.Add("10000.00");
                basket.Add(";");
                basket.Add("buah");
                basket.Add("10000.00");
                basket.Add("1");
                basket.Add("10000.00");
                basket.Add(";");
                string JSONBasket = Library.formatBasket(Helper.ListToJson(basket));

                Dictionary<string, string> customer = new Dictionary<string, string>();
                customer.Add("name", "TEST NAME");
                customer.Add("data_phone", "08121111111");
                customer.Add("data_email", "test@test.com");
                customer.Add("data_address", "bojong gede #1 08/01");
                string JSONCustomer = Helper.DictionaryToJson(customer);

                string[] binFilter = { "411111", "548117", "433???6", "41*3" };

                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("req_token_id", Request.Form["doku_token"]);
                data.Add("req_pairing_code", Request.Form["doku_pairing_code"]);
                data.Add("req_bin_filter", binFilter);
                data.Add("req_customer", JSONCustomer);
                data.Add("req_basket", JSONBasket);
                data.Add("req_words", words);
                #endregion

                string JSONData = Helper.DictionaryToJson(data);
                Debug.Write(JSONData);

                Dictionary<string, object> predata = new Dictionary<string, object>();
                predata.Add("data", JSONData);

                string responsePrePayment = Doku_Api.doPrePayment(predata);

                dynamic results = Newtonsoft.Json.JsonConvert.DeserializeObject(responsePrePayment);
                if (results != null){
                    var msg = results.res_response_msg;
                    var code = results.res_response_code;
                    if (msg != null && code != null)
                    {
                        if (code.Value == "0000" && msg.Value == "SUCCESS") //Prepayment Success
                        {
                            #region data post
                            Dictionary<string, object> dataPayment = new Dictionary<string, object>();
                            dataPayment.Add("req_mall_id", Request.Form["doku_mall_id"]);
                            dataPayment.Add("req_chain_merchant", Request.Form["doku_chain_merchant"]);
                            dataPayment.Add("req_amount", Request.Form["doku_amount"]);
                            dataPayment.Add("req_words", words);
                            dataPayment.Add("req_words_raw", Library.doCreateWordsRaw(param));
                            dataPayment.Add("req_purchase_amount", Request.Form["doku_amount"]);
                            dataPayment.Add("req_trans_id_merchant", Request.Form["doku_invoice_no"]);
                            dataPayment.Add("req_request_date_time", Helper.getDate());
                            dataPayment.Add("req_currency", Request.Form["doku_currency"]);
                            dataPayment.Add("req_purchase_currency", Request.Form["doku_currency"]);
                            dataPayment.Add("req_session_id", Helper.GetSHA1HashData(Helper.getDate()));
                            dataPayment.Add("req_name", customer["name"]);
                            dataPayment.Add("req_payment_channel", 15);
                            dataPayment.Add("req_basket", JSONBasket);
                            dataPayment.Add("req_email", customer["data_email"]);
                            dataPayment.Add("req_token_id", Request.Form["doku_token"]);
                            dataPayment.Add("req_mobile_phone", customer["data_phone"]);
                            dataPayment.Add("req_address", customer["data_address"]);
                            #endregion

                            string JSONDataPayment = Helper.DictionaryToJson(dataPayment);
                            Debug.Write(JSONDataPayment);
                            Dictionary<string, object> dict_dataPayment = new Dictionary<string, object>();
                            dict_dataPayment.Add("data", JSONDataPayment);

                            string responsePayment = Doku_Api.doPayment(dict_dataPayment);
                            results = Newtonsoft.Json.JsonConvert.DeserializeObject(responsePayment);

                            if (results != null)
                            {
                                msg = results.res_response_msg;
                                code = results.res_response_code;

                                if (msg != null && code != null && code.Value == "0000" && msg.Value == "SUCCESS")
                                {
                                    #region MERCHANT PROCESS
                                    //merchant process
                                    //do what you want to do
                                    #endregion

                                    #region token
                                    //process tokenization
                                    if (results.res_bundle_token != null)
                                    {
                                        dynamic tokenPayment = Newtonsoft.Json.JsonConvert.DeserializeObject(results.res_bundle_token.Value);
                                        if (tokenPayment != null)
                                        {
                                            if (tokenPayment.res_token_code == "0000")
                                            {
                                                //save token
                                                string getTokenPayment = tokenPayment.res_token_payment.Value;
                                            }
                                        }
                                        else
                                        {
                                            Literal1.Text = "Response Null";
                                        }
                                    }
                                    #endregion

                                    //redirect process to doku
                                    string urlBased = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf("/example/"));
                                    results.res_redirect_url = urlBased + "/example-payment/merchant-redirect-example.aspx";
                                    results.res_show_doku_page = true; //true if you want to show doku page first before redirecting to redirect url

                                    Literal1.Text = Newtonsoft.Json.JsonConvert.SerializeObject(results);
                                }
                                else
                                {
                                    Literal1.Text = Newtonsoft.Json.JsonConvert.SerializeObject(results);
                                }
                            }
                            else
                            {
                                Literal1.Text = "Response Null";
                            }
                        }
                        else
                        {
                            Literal1.Text = Newtonsoft.Json.JsonConvert.SerializeObject(results);
                        }
                    }
                    else
                    {
                        Literal1.Text = "Response Null";
                    }
                }
                else
                {
                    Literal1.Text = "Response Null";
                }
            }
        }
    }
}