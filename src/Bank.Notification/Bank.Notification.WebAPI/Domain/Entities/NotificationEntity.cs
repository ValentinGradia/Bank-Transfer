using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace Bank.Notification.WebAPI.Domain.Entities;

//Al usar una bbdd no sql, especificamos las propiedades de la entidad con los atributos de JsonProperty para mapear correctamente los campos en la base de datos. Esto es especialmente útil para
//mantener la consistencia entre el modelo de datos y la estructura de la base de datos, facilitando así las operaciones de lectura y escritura.
public class NotificationEntity
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("correlationId")]
    public string CorrelationId { get; set; }
    
    [JsonProperty("notificationDate")]
    public DateTime NotificationDate { get; set; }
    
    [JsonProperty("customerId")]
    public int CustomerId { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; } //El tipo de notificación (por ejemplo, "Email", "SMS", "Push")
    
    [JsonProperty("content")]
    public string Content { get; set; } //El CONTENIDO de la notificación
    
    [JsonProperty("transactionStatus")]
    public bool TransactionStatus { get; set; } //El estado de la transacción relacionada (por ejemplo, "Success", "Failure")

}