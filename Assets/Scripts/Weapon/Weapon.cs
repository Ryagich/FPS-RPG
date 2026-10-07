using VContainer;
using System;
using Characters;
using Inventory;
using Inventory.Pools;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;
using Random = UnityEngine.Random;

namespace Weapon
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Weapon : ITickable, IFixedTickable, IItem
    {
        public event Action<int> ValueChanged;
        public event Action<ShootingMode> ShootingModChanged;
        public event Action<Vector2> RequestRecoil;
        public event Action Shot;
        public event Action EmptyShot;
        
        public GameObject GameObject { get; set; }
        public WeaponPresentation Presentation { get; }

        public readonly WeaponConfig Config;
        private readonly Transform aimTransform;
        private readonly AttachmentsController attachmentsC;
        private readonly ProjectilesPool projectilesPool;
        private readonly IPublisher<RecoilMessage> requestRecoilPublisher;
        private readonly CharacterState characterState;
        private readonly IPublisher<ShotFiredMessage> shootPublisher;

        private readonly int tracerInterval = 1;
        private readonly float timeBetweenShots;

        private int interval;
        private int burstShotsLeft;
        private float lastFireTime;
        private Vector2 recoil;

        public int Value { get; private set; }
        public bool IsShooting { get; private set; }
        public ShootingMode ShootingMode { get; private set; }
        public float CurrentSpread { get; private set; }

        public int NeedAmmo() => Config.GetMaxCapacity() - Value;

        public Weapon
            (
                WeaponConfig config,
                GameObject gameObject,
                WeaponPresentation presentation,
                CharacterState characterState,
                [Key("AimTransform")] Transform aimTransform,
                IPublisher<ShotFiredMessage> shootPublisher,
                AttachmentsController attachmentsC,
                ProjectilesPool projectilesPool,
                IPublisher<RecoilMessage> requestRecoilPublisher
            )
        {
            Config = config;
            GameObject = gameObject;
            Presentation = presentation;
            
            this.characterState = characterState;
            this.shootPublisher = shootPublisher;
            this.attachmentsC = attachmentsC;
            this.projectilesPool = projectilesPool;
            this.requestRecoilPublisher = requestRecoilPublisher;

            this.aimTransform = aimTransform;
            if (config.Modes.Count is 0)
            {
                throw new AggregateException("The weapon does not have any possible firing modes set.");
            }
            ShootingMode = config.Modes.Contains(ShootingMode.Auto)
                               ? ShootingMode.Auto
                               : config.Modes.Contains(ShootingMode.Single)
                                   ? ShootingMode.Single
                                   : ShootingMode.Burst;
            ShootingModChanged?.Invoke(ShootingMode);

            timeBetweenShots = 60.0f / config.GetRPM();
            //CurrentSpread = _config.GetCurrentRecoilSettings(characterState.IsAiming).RecoilChillCoefficient; //???
            Value = Config.GetMaxCapacity();
        }

        public bool TryChangeValue(int amount)
        {
            if (Value + amount < 0
             || Value + amount > Config.GetMaxCapacity())
            {
                return false;
            }
            Value += amount;
            ValueChanged?.Invoke(Value);
            return true;
        }

        public bool TrySwitchShootingMode()
        {
            if (Config.Modes.Count == 1)
            {
                return false;
            }
            if (IsShooting)
            {
                return false;
            }
            var i = Config.Modes.IndexOf(ShootingMode) + 1;
            if (i >= Config.Modes.Count)
            {
                i = 0;
            }
            ShootingMode = Config.Modes[i];
            ShootingModChanged?.Invoke(ShootingMode);
            return true;
        }

        public void SetMovementSpeed(Vector3 motion)
        {
            var summ = MathF.Abs(motion.x) + MathF.Abs(motion.z);
            recoil += new Vector2(summ / 2, summ / 2) * Config.MovementMultiply;
            recoil =
                new Vector2(Mathf.Clamp(recoil.x, .0f, Config.GetCurrentRecoilSettings(characterState.IsAiming).MaxRecoilPower.x),
                            Mathf.Clamp(recoil.y, .0f,
                                        Config.GetCurrentRecoilSettings(characterState.IsAiming).MaxRecoilPower.y));
            CurrentSpread = Mathf.Max(
                                      Config.GetCurrentRecoilSettings(characterState.IsAiming).Spread
                                    + (recoil.y * Config.GetCurrentRecoilSettings(characterState.IsAiming)
                                                        .RecoilSpreadMultiplier),
                                      Config.GetCurrentRecoilSettings(characterState.IsAiming).Spread
                                     );
        }

#region Shooting

        public void Tick()
        {
            if (GameObject.activeInHierarchy && Value > 0)
            {
                TryShoot();
            }
        }

        public void FixedTick()
        {
            if (!GameObject.activeInHierarchy)
                return;
            recoil *= Config.GetCurrentRecoilSettings(characterState.IsAiming).RecoilChillCoefficient;
            CurrentSpread = Mathf.MoveTowards(
                                              CurrentSpread,
                                              Config.GetCurrentRecoilSettings(characterState.IsAiming).Spread,
                                              Config.GetCurrentRecoilSettings(characterState.IsAiming).SpreadChillCoefficient *
                                              Time.deltaTime
                                             );
        }

        private void TryShoot()
        {
            if (Time.time - lastFireTime < timeBetweenShots)
            {
                return;
            }
            switch (ShootingMode)
            {
                case ShootingMode.Single:
                    if (IsShooting)
                    {
                        Shoot();
                        StopShoot();
                    }
                    break;
                case ShootingMode.Burst:
                    if (burstShotsLeft > 0)
                    {
                        Shoot();
                        burstShotsLeft--;
                        if (burstShotsLeft <= 0)
                        {
                            StopShoot();
                        }
                    }
                    break;
                case ShootingMode.Auto:
                    if (IsShooting)
                    {
                        Shoot();
                    }
                    break;
            }
            if (Value <= 0)
            {
                burstShotsLeft = 0;
                StopShoot();
            }
        }

        private void Shoot()
        {
            interval++;
            var recoilSettings = Config.GetCurrentRecoilSettings(characterState.IsAiming);
            // считаем разброс
            CurrentSpread = Mathf.Max(
                                      recoilSettings.Spread + (recoil.y * recoilSettings.RecoilSpreadMultiplier),
                                      recoilSettings.Spread
                                     );
            var spread = Random.insideUnitCircle * CurrentSpread;
            var shootingTransform = aimTransform;
            if (characterState.IsAiming && attachmentsC.Scope != null && attachmentsC.Scope.CenterTransform)
            {
                shootingTransform = attachmentsC.Scope.CenterTransform;
            }
            var shootingPos = shootingTransform.position;
            // направление от КАМЕРЫ (центр экрана + разброс)
            var cameraDir = (shootingTransform.forward
                           + shootingTransform.right * spread.x
                           + shootingTransform.up * spread.y).normalized;

            // ---------- ВАЖНО ----------
            // теперь строим луч именно от камеры
            // var cameraRay = new Ray(shootingPos, cameraDir);

            var hitPoint = shootingPos + cameraDir * 200.0f; // если не попали — точка вдали
            // var layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
            //
            // if (Physics.Raycast(cameraRay, out var hit, 200.0f, layerMask))
            // {
            //     hitPoint = hit.point;
            //
            //     GlobalMessagePipe.GetRequestAllHandler<PlayerHitMessage, Unit>().InvokeAll(new PlayerHitMessage(hit, Config));
            // }

            // теперь считаем направление из Muzzle к точке попадания
            var shotPoint = attachmentsC.Muzzle.ShotPoint;
            var pos = shotPoint.position;
            var finalDir = (hitPoint - pos).normalized;
            var direction = Quaternion.LookRotation(finalDir);

            // создаем пулю
            if (interval >= tracerInterval)
            {
                var p = projectilesPool.Get(pos, direction, Config);
                var rb = p.Body;
                rb.linearVelocity = finalDir * Config.ProjectileSpeed;
                interval = 0;
            }

            attachmentsC.Muzzle.Effect();
            GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                             .Publish(new PlaySoundMessage(Config.ShootSound.SoundSettings,
                                                           attachmentsC.Muzzle.ShotPoint.position, null));
            // shootSoundPublisher.Publish(new PlaySoundMessage(Config.ShootSound.SoundSettings, attachmentsC.Muzzle.ShotPoint.position, null));

            lastFireTime = Time.time;
            TryChangeValue(-1);
            recoil += recoilSettings.RecoilPower;
            recoil = new Vector2(
                                 Mathf.Clamp(recoil.x, .0f, recoilSettings.MaxRecoilPower.x),
                                 Mathf.Clamp(recoil.y, .0f, recoilSettings.MaxRecoilPower.y)
                                );
            RequestRecoil?.Invoke(recoil);
            requestRecoilPublisher.Publish(new RecoilMessage(recoil, Config.ShakeSettings));
            shootPublisher.Publish(new ShotFiredMessage(this, direction));
            Shot?.Invoke();
        }

        public void StartShoot()
        {
            if (Value <= 0)
            {
                GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                                 .Publish(new PlaySoundMessage(Config.EmptyShootSound.SoundSettings,
                                                               attachmentsC.Muzzle.ShotPoint.position, null));
                // shootSoundPublisher.Publish(new PlaySoundMessage(Config.EmptyShootSound.SoundSettings, attachmentsC.Muzzle.ShotPoint.position, null));
                EmptyShot?.Invoke();
                return;
            }
            if (ShootingMode.Equals(ShootingMode.Burst))
            {
                burstShotsLeft = 3;
            }
            IsShooting = true;
        }

        public void StopShoot()
        {
            IsShooting = false;
            burstShotsLeft = 0;
        }

#endregion

        public void Activate()
        {
            GameObject.SetActive(true);
        }

        public void Disable()
        {
            GameObject.SetActive(false);
        }

        public LifetimeScope GetDropPrefab()
        {
            return Config.DropWeaponPref;
        }
    }

    public sealed record PlayerHitMessage(RaycastHit Hit, WeaponConfig WeaponConfig)
    {
        public RaycastHit Hit { get; } = Hit;
        public WeaponConfig WeaponConfig { get; } = WeaponConfig;
    }

}