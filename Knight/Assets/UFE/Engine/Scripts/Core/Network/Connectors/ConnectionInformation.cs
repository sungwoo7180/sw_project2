public class ConnectionInformation{
	public int port = 0;
	public string publicAddress = null;
	public string privateAddress = null;

	public ConnectionInformation(){}

	public ConnectionInformation(
		string privateAddress, 
		string publicAddress, 
		int port
	){
		this.privateAddress = privateAddress;
		this.publicAddress = publicAddress;
		this.port = port;
	}
}
