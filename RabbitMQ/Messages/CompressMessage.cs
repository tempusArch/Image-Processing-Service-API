namespace ImageProcessingServiceAPI.Application;

public class CompressMessage {
    public string UserId {get; set;}
    
    public string ImageId {get; set;}

    public int Quality {get; set;}

    public string ResultName {get; set;}

}