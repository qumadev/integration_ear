﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Windows.Forms;
using System.Net.Mail;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Net;
using System.Configuration;
using System.Net.Http;
using STR_SERVICE_INTEGRATION_EAR.EL.Requests;

namespace STR_SERVICE_INTEGRATION_EAR.BL
{
    public class EnviarEmail
    {
        public static string CorreoEmisor = string.Empty;
        public static string CorreoClave = string.Empty;
        public static string Link = string.Empty;
        public static string Logo = string.Empty;
        public static string Gif = string.Empty;
        public static bool Prod = false;
        public static string NombreEmisor = string.Empty;
        public static string CorreoPrueba = string.Empty;
        public EnviarEmail()
        {
            CorreoEmisor = ConfigurationManager.AppSettings["CorreoEmisor"].ToString();
            CorreoClave = ConfigurationManager.AppSettings["CorreoClave"].ToString();
            Logo = ConfigurationManager.AppSettings["Logo"].ToString();
            Link = ConfigurationManager.AppSettings["LinkPortal"].ToString();
            Gif = ConfigurationManager.AppSettings["Gif"].ToString();
            Prod = ConfigurationManager.AppSettings["Prod"].ToString() == "1";
            CorreoPrueba = ConfigurationManager.AppSettings["correo_receptor_prueba"].ToString();
            NombreEmisor = ConfigurationManager.AppSettings["nombre_emisor"];
        }
        public void EnviarInformativo(string correoReceptor, string nombreEmpleado, bool solicitud, string codigo, string codigoRendicion,
            string fechaRegistro, bool aceptado, string comentarios)
        {
            try
            {
                string rutaArchivo = ConfigurationManager.AppSettings["RutaArchivoInformacion"].ToString();

                // Lee el contenido del archivo HTML
                // Lee el contenido del archivo HTML
                string Themessage = LeerArchivoHtml(rutaArchivo);

                MailMessage msg = new MailMessage();

                msg.From = new MailAddress(CorreoEmisor, NombreEmisor, Encoding.UTF8);

                msg.To.Add(Prod ? correoReceptor : CorreoPrueba);

                //msg.Subject = "Portal Entreg a Rendir - Estado de Rendición";
                msg.Subject = "Portal Entrega a Rendir - Confirmación de Rendición";
                msg.SubjectEncoding = Encoding.UTF8;
                string body = string.Empty;

                body = Themessage.ToString();

                body = body.Replace("[$Nombres]", nombreEmpleado);
                body = body.Replace("[$Transaccion]", solicitud ? "Solicitud de Rendición" : "Rendición");
                body = body.Replace("[$Codigo]", codigo);
                body = body.Replace("[$CodigoRendicion]", !string.IsNullOrEmpty(codigoRendicion) ? "" + codigoRendicion : "");
                body = body.Replace("[$FechaAceptacion]", fechaRegistro);
                body = body.Replace("[$Estado]", aceptado ? "Aceptado" : "Rechazado");
                body = body.Replace("[$Logo]", Logo);
                body = body.Replace("[$Link]", Link);
                body = body.Replace("[$Gif]", aceptado ? Gif : "");
                body = body.Replace("[$Motivo]", comentarios);

                msg.IsBodyHtml = true;
                msg.Body = body;

                EnvioDocumento(msg);
                // Imprime el contenido HTML en la consola
                Console.WriteLine("Contenido HTML leído:\n");

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void EnviarConfirmacion(string correoReceptor, string nombreReceptor,
            string nombreEmpleado, bool solicitud, string codigo, string codigoRendicion,
            string fechaRegistro, string estado, string area = "", string aprobadorId = "", string rendicionId = "", string areaEmpleado = "")
        {
            string linkAcepta = string.Empty;
            string linkRechaza = string.Empty;
            try
            {

                string rutaArchivo = ConfigurationManager.AppSettings["RutaArchivoConfirmacion"].ToString();

                // Lee el contenido del archivo HTML
                string Themessage = LeerArchivoHtml(rutaArchivo);

                MailMessage msg = new MailMessage
                {
                    From = new MailAddress(CorreoEmisor, NombreEmisor, Encoding.UTF8)
                };
                msg.To.Add(Prod ? correoReceptor : CorreoPrueba);

                //msg.Subject = "Portal Entreg a Rendir - Estado de Rendición";
                msg.Subject = "Portal Entrega a Rendir - Confirmación de Rendición";
                msg.SubjectEncoding = Encoding.UTF8;
                string body = string.Empty;

                body = Themessage.ToString();

                // Como Hacer el link de Acepta y rechazo  -- rutaApi - linkAcepta GENERAR EL TOKEN
                //linkAcepta = rutaApi + $"/acepta?id=${codigo}&aprobadorId=${aprobadorId}&areaAprobador=${area}";
                //linkRechaza = rutaApi + $"/rechazar?id=${codigo}&aprobadorId=${aprobadorId}&areaAprobador=${area}";
                Token token = new Token();
                linkAcepta = Link + (solicitud ? "solicitudes/" : "rendiciones/") + (!string.IsNullOrEmpty(aprobadorId) ? token.GenerarToken(solicitud ? codigo : rendicionId, aprobadorId, area.ToString(), "aceptar", estado, solicitud ? rendicionId : codigo, areaEmpleado) : "");
                linkRechaza = Link + (solicitud ? "solicitudes/" : "rendiciones/") + (!string.IsNullOrEmpty(aprobadorId) ? token.GenerarToken(solicitud ? codigo : rendicionId, aprobadorId, area.ToString(), "rechazar", estado, solicitud ? rendicionId : codigo, areaEmpleado) : "");

                body = body.Replace("[$NombreEmisor]", nombreReceptor);     // Parametro
                body = body.Replace("[$NombreReceptor]", nombreEmpleado); // Parametro
                body = body.Replace("[$Transaccion]", solicitud ? "Solicitud de Rendición" : "Rendición"); // Parametro
                body = body.Replace("[$Codigo]", codigo);                    // Parametor
                body = body.Replace("[$CodigoRendicion]", string.IsNullOrEmpty(codigoRendicion) ? "" : "con Número de rendición: " + codigoRendicion);                  // Parametro
                body = body.Replace("[$FechaRegistro]", fechaRegistro);          // Parametro
                body = body.Replace("[$Estado]", estado);                  // Parametro
                body = body.Replace("[$Logo]", Logo); // Config
                body = body.Replace("[$Link]", Link);                                                               // Config
                body = body.Replace("[$Gif]", Gif);                   // Config
                body = body.Replace("[$LinkAceptacion]", linkAcepta);                   // Parametro
                body = body.Replace("[$LinkRechazo]", linkRechaza);                           // Parametro

                msg.IsBodyHtml = true;
                msg.Body = body;

                EnvioDocumento(msg);
                // Imprime el contenido HTML en la consola
                Console.WriteLine("Contenido HTML leído:\n");
                // Console.WriteLine(contenidoHtml);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void EnviarError(bool solicitud, string codigoRendicion, string codigo, string fechaRegistro, string mensajeError)
        {
            try
            {
                List<string> admins = new List<string>();
                string rutaArchivo = ConfigurationManager.AppSettings["RutaArchivoError"].ToString();
                string _admins = ConfigurationManager.AppSettings["correo_receptor_errores"].ToString();
                admins = _admins.Split(',').ToList();

                // Obtiene administradores

                string Themessage = LeerArchivoHtml(rutaArchivo);

              
                for (int i = 0; i < admins.Count; i++)
                {
                    MailMessage msg = new MailMessage
                    {
                        From = new MailAddress(CorreoEmisor, NombreEmisor, Encoding.UTF8)
                    };

                    msg.To.Add(admins[i]);

                    //msg.Subject = "Portal Entreg a Rendir - Estado de Rendición";
                    msg.Subject = "Portal Entrega a Rendir - Error en Migración";
                    msg.SubjectEncoding = Encoding.UTF8;
                    string body = string.Empty;

                    body = Themessage.ToString();

                    body = body.Replace("[$Transaccion]", solicitud ? "Solicitud de Rendición" : $"Rendición °{codigoRendicion}"); // Se identifica si hay numero de rendición
                    body = body.Replace("[$Codigo]", codigo);                    // Parametor
                    body = body.Replace("[$FechaAceptacion]", fechaRegistro);          // Parametro
                    body = body.Replace("[$Logo]", Logo); // Config
                    body = body.Replace("[$Link]", Link);                                                               // Config
                    body = body.Replace("[$Gif]", Gif);                   // Config
                    body = body.Replace("[$Motivo]", mensajeError);

                    msg.IsBodyHtml = true;
                    msg.Body = body;

                    EnvioDocumento(msg);
                    // Imprime el contenido HTML en la consola
                    Console.WriteLine("Contenido HTML leído:\n");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void EnvioDocumento(MailMessage msg)
        {
            // Obtener el PORT y HOST
            string host = ConfigurationManager.AppSettings["correo_host"].ToString();
            int port = int.Parse(ConfigurationManager.AppSettings["correo_port"].ToString());
            bool enableSsl = ConfigurationManager.AppSettings["correo_ssl"] == "1";

            msg.BodyEncoding = Encoding.UTF8;
            msg.Priority = MailPriority.Normal;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(CorreoEmisor, CorreoClave);
            client.Host = host;
            client.Port = port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = enableSsl;

            client.Send(msg);
            client.Dispose();

        }

        static string LeerArchivoHtml(string rutaArchivo)
        {
            // Verifica si el archivo existe
            if (File.Exists(rutaArchivo))
            {
                // Lee el contenido del archivo y lo devuelve como una cadena
                return File.ReadAllText(rutaArchivo);
            }
            else
            {
                throw new FileNotFoundException($"El archivo HTML no fue encontrado en la ruta: {rutaArchivo}");
            }
        }
    }
}