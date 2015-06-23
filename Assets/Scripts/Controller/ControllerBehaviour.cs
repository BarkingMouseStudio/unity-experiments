using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Responsible for sending inputs and applying torque,
// controlled by the neural network.
public class ControllerBehaviour : MonoBehaviour {

  public NetworkIO networkIO;

  WheelJoint2D wheelJoint;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  EvaluationBehaviour evaluation;

  void Awake() {
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();
    evaluation = GetComponent<EvaluationBehaviour>();

    var genotype = CommonGenotype.FromJSON(
      "[[0.2026012,0.9384483,0.03939772,0.8526343],[0.7229823,0.5886511,0.4537784,0.5277259],[0.42949,0.1607368,0.5565715,0.2380508],[0.157934,0.1130141,0.5414627,0.1508785],[0.4366309,0.3064492,0.6648943,0.1814191],[0.03372967,0.8150965,0.9325932,0.8720315],[0.9823589,0.7625747,0.7333834,0.9892329],[0.2693458,0.4388986,0.8064783,0.5650929],[0.6260212,0.101186,0.5430595,0.8847258],[0.1539515,0.194923,0.09496701,0.4345754],[0.6369064,0.7468157,0.2617623,0.7722585],[0.313776,0.2327508,0.7889553,0.3568241],[0.0179075,0.5586967,0.9541587,0.5574451],[0.1923002,0.389964,0.5413333,0.8699466],[0.9713854,0.7558885,0.8376477,0.6657879],[0.1882801,0.1464516,0.9157932,0.622696],[0.9200393,0.979461,0.6547936,0.7754399],[0.1461004,0.07126689,0.2254419,0.3638806],[0.8176696,0.5006893,0.352112,0.0529747],[0.1593872,0.9859388,0.7385334,0.701133],[0.771717,0.6189094,0.3831339,0.9112674],[0.293516,0.7438201,0.3602398,0.08425891],[0.8720826,0.8264706,0.8789465,0.5840917],[0.7017523,0.835281,0.7380565,0.2291934],[0.3461,0.9350119,0.6211964,0.7487488],[0.1453851,0.99913,0.9289224,0.06601477],[0.3515363,0.1878452,0.5394248,0.5651725],[0.1852478,0.2571866,0.1553698,0.08942032],[0.2029762,0.03823137,0.6758928,0.635819],[0.8611252,0.6368992,0.6371398,0.3580449],[0.04311335,0.9197658,0.3512501,0.09461665],[0.7646538,0.6801214,0.104715,0.8256192],[0.7834424,0.2134356,0.7355831,0.5826568],[0.1716014,0.3585506,0.5709234,0.2292454],[0.3618187,0.8350798,0.4844863,0.6658152],[0.5773845,0.4067758,0.05612779,0.7305868],[0.4055266,0.7382334,0.8628912,0.4127483],[0.3815938,0.4506201,0.5001996,0.5008004],[0.8369169,0.4659887,0.6812906,0.3511247],[0.2680042,0.434764,0.684976,0.06218922],[0.6658319,0.07901847,0.9612334,0.9933234],[0.9570839,0.805961,0.5037388,0.3419976],[0.4109166,0.2835912,0.6423111,0.9905427],[0.9546264,0.8073589,0.08891773,0.5928021],[0.2039348],[0.1951494],[0.2603856],[0.1989967],[0.5809055],[0.4897276],[0.7394886],[0.4732878],[0.252896],[0.4360314],[0.4306236],[0.5327201],[0.4382961],[0.4438975],[0.4079326],[0.8334979],[0.206939],[0.4831333],[0.06347561],[0.5284002],[0.7092574],[0.9791676],[0.9802953],[0.1626992],[0.747957],[0.3296247],[0.2656731],[0.8337256],[0.6932116],[0.7068876],[0.5836996],[0.5938048],[0.1700474],[0.1132947],[0.9376198],[0.3697588],[0.4031433],[0.8867334],[0.6236637],[0.6650722],[0.5776152],[0.4505353],[0.04004121],[0.7340348],[0.981849],[0.9291252],[0.454852],[0.9349904],[0.1720594],[0.9316601],[0.465673],[0.358964],[0.8819611],[0.01482654],[0.3427363],[0.2967227],[0.9220047],[0.6397337],[0.5777827],[0.002680779],[0.02506745],[0.2090189],[0.2365012],[0.9517545],[0.7589777],[0.5781432],[0.3362967],[0.3373597],[0.9741356],[0.3326687],[0.782361],[0.8807619],[0.07388759],[0.4593187],[0.2278073],[0.7080085],[0.06585312],[0.8657613],[0.1554618],[0.64862],[0.0002928972],[0.74306],[0.4284859],[0.6514909],[0.7777656],[0.4904981],[0.3003224],[0.5259128],[0.4750627],[0.3923989],[0.709671],[0.8669399],[0.6672883],[0.1603093],[0.8617131],[0.05740023],[0.1859366],[0.1987278],[0.1149065],[0.5650818],[0.06258403],[0.8347398],[0.3076577],[0.001405239],[0.6904061],[0.2739775],[0.2311347],[0.02475727],[0.9406049],[0.2245969],[0.2307203],[0.02409041],[0.7556758],[0.1764878],[0.7494348],[0.07282627],[0.1446667],[0.7036097],[0.6202653],[0.07941329],[0.03200173],[0.1280665],[0.6808475],[0.1254523],[0.9875402],[0.4933031],[0.6134891],[0.929934],[0.3931779],[0.5193703],[0.03230107],[0.01896691],[0.7916701],[0.4368432],[0.2737226],[0.6780534],[0.1100559],[0.8192326],[0.2565388],[0.9292722],[0.6592387],[0.2303412],[0.1839687],[0.1066566],[0.6406368],[0.4537049],[0.1509864],[0.4087437],[0.2908719],[0.4386858],[0.3253986],[0.1430862],[0.4550545],[0.03709686],[0.1070498],[0.7625184],[0.958528],[0.4920638],[0.4707815],[0.1285995],[0.3129723],[0.3216752],[0.2585939],[0.6460059],[0.05937686],[0.5207092],[0.2455964],[0.2914489],[0.4962155],[0.1127168],[0.6562026],[0.06592417],[0.2399701],[0.4910392],[0.20198],[0.4960688],[0.4638653],[0.6494894],[0.9533195],[0.8628549],[0.8201841],[0.8084534],[0.633875],[0.2082901],[0.5965403],[0.831893],[0.5436754],[0.9147266],[0.5309665],[0.6603676],[0.1079478],[0.5888198],[0.7520387],[0.7915933],[0.6925715],[0.5521834],[0.008895397],[0.3424557],[0.7983162],[0.1176761],[0.3861319],[0.5861374],[0.08249319],[0.3362119],[0.2731284],[0.1238986],[0.9645718],[0.7092717],[0.006763339],[0.6727108],[0.7596042],[0.9315367],[0.4406282],[0.2612416],[0.770756],[0.9922525],[0.9334637],[0.187515],[0.9806188],[0.05315793],[0.2618948],[0.2845812],[0.5630675],[0.6720212],[0.01858926],[0.3628153],[0.8415555],[0.2913556],[0.9780964],[0.9282323],[0.9656047],[0.8680161],[0.03987229],[0.8400747],[0.7812888],[0.932283],[0.6528397],[0.008933306],[0.2716576],[0.991969],[0.3530732],[0.3682269],[0.975122],[0.4355542],[0.1405185],[0.4800881],[0.2844512],[0.9535781],[0.4666931],[0.4707852],[0.2384111],[0.06500638],[0.6442726],[0.5398964],[0.3563569],[0.4295879],[0.5632626],[0.1216625],[0.6335288],[0.2361468],[0.6270555],[0.5566868],[0.1571693],[0.3603874],[0.1463189],[0.54422],[0.9578182],[0.2495003],[0.905399],[0.005890489],[0.7359749],[0.4127052],[0.1974151],[0.368905],[0.5133958],[0.5142574],[0.830192],[0.8595769],[0.6975716],[0.1427182],[0.3705578],[0.9586377],[0.9655814],[0.527695],[0.6346636],[0.9631616],[0.8523347],[0.3921408]]"
    );

    networkIO = new NetworkIO(genotype.GetPhenotype());
  }

  void OnDespawned() {
    // Reset motor speed
    SetMotorSpeed(0);
  }

  void SetMotorSpeed(float speed) {
    var motor = wheelJoint.motor;
  	motor.motorSpeed = speed;
  	wheelJoint.motor = motor;
  }

	void FixedUpdate() {
    if (evaluation.IsComplete) {
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    var speed = networkIO.Send(thetaLower, thetaDotLower, x, xDot);
    // Debug.LogFormat("speed: {0}, thetaLower: {1}, thetaDotLower: {2}, x: {3}, xDot: {4}", speed, thetaLower, thetaDotLower, x, xDot);

    // Update motor speed
    SetMotorSpeed(speed);
	}
}
